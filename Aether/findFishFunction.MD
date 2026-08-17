 private async Task ScanAreaForFishColorsAsync()
        {
            try
            {

                changeLogText("renk araması ve tıklama başladı");
                changeLogText(Cursor.Position.X.ToString() + " / " + Cursor.Position.Y.ToString());

                // İstediğin gibi 300x300 tarama alanı boyutu
                // int scanSize = 140;
                //int startPointX = lockedPosition.X - (scanSize / 2);
                //int startPointY = lockedPosition.Y - (scanSize / 2);
                int scanSize = 140;
                int startPointX = 252 - (scanSize / 2);
                int startPointY = 218 - (scanSize / 2);

                int targetPixelX = -1;
                int targetPixelY = -1;
                bool isColorFound = false;

                using (Bitmap screenshotBmp = new Bitmap(scanSize, scanSize))
                {
                    using (Graphics screenGraphics = Graphics.FromImage(screenshotBmp))
                    {
                        // Kilitli merkezin etrafındaki 300x300'lük alanı fotoğraflıyoruz
                        screenGraphics.CopyFromScreen(startPointX, startPointY, 0, 0, screenshotBmp.Size);
                    }

                    // Pikselleri RAM üzerinde kilitliyoruz (Işık hızı tarama tamponu)
                    BitmapData bitmapDataBuffer = screenshotBmp.LockBits(
                        new Rectangle(0, 0, screenshotBmp.Width, screenshotBmp.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

                    // 💡 KURAL GEREĞİ: Unsafe blok sadece tarama yapar, asla await içermez (CS4004 hatasını önler)
                    unsafe
                    {
                        byte* currentPixelPointer = (byte*)bitmapDataBuffer.Scan0;

                        for (int y = 0; y < screenshotBmp.Height; y++)
                        {
                            for (int x = 0; x < screenshotBmp.Width; x++)
                            {
                                byte currentBlue = currentPixelPointer[0];
                                byte currentGreen = currentPixelPointer[1];
                                byte currentRed = currentPixelPointer[2];
                                byte currentAlpha = currentPixelPointer[3];

                                // 4 byte veriyi tek bir 32-bit int içine bit kaydırmayla paketliyoruz
                                int currentPixelColorAsInt = (currentAlpha << 24) | (currentRed << 16) | (currentGreen << 8) | currentBlue;

                                // Negatif renk listemizdeki değerlerden biriyle çakıştı mı?
                                if (Array.IndexOf(fishTargetColorValues, currentPixelColorAsInt) != -1)
                                {
                                    targetPixelX = startPointX + x;
                                    targetPixelY = startPointY + y;
                                    isColorFound = true;
                                    break;
                                }

                                currentPixelPointer += 4;
                            }
                            if (isColorFound) break;
                        }
                    }

                    // Tarama bittiği an bellek kilidini serbest bırakıyoruz
                    screenshotBmp.UnlockBits(bitmapDataBuffer);
                }

                // Unsafe alandan tamamen çıktıktan sonra asenkron donanımsal tıklamayı yapıyoruz
                if (isColorFound)
                {
                    await ClickMouseAtCoordinatesAsync(targetPixelX, targetPixelY);
                }
                else
                {
                    changeLogText("balık bulunamadı");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
            changeLogText("renk araması ve tıklama bitti");

        }