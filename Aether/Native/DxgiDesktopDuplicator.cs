using Aether.Native;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Aether.Native
{
    /// <summary>
    /// Windows DXGI (DirectX Graphics Infrastructure) Desktop Duplication API yöneticisi.
    /// GPU sürücüsü seviyesinde masaüstü görüntüsünü doğrudan VRAM/DWM üzerinden okur.
    /// Oyun pencerelerine veya HWND tutacaklarına doğrudan API çağrısı yapmadığı için Anti-Cheat
    /// korumalarının (siyah ekran engeli vb.) radarına takılmadan en güvenli ve en yüksek performanslı
    /// ekran yakalama yöntemidir.
    /// </summary>
    public sealed class DxgiDesktopDuplicator : IDisposable
    {
        #region Native Structs & Constants

        private static readonly Guid IID_IDXGIDevice = new Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c");
        private static readonly Guid IID_IDXGIOutput1 = new Guid("00ab88a3-4e05-4b3e-b882-9e012a2e1d70");
        private static readonly Guid IID_ID3D11Texture2D = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

        private const uint D3D11_SDK_VERSION = 7;
        private const int D3D_DRIVER_TYPE_HARDWARE = 1;
        private const uint D3D11_CREATE_DEVICE_BGRA_SUPPORT = 0x0020;

        private const int DXGI_FORMAT_B8G8R8A8_UNORM = 87;
        private const int D3D11_USAGE_STAGING = 3;
        private const uint D3D11_CPU_ACCESS_READ = 0x00020000;
        private const int D3D11_MAP_READ = 1;

        private const int DXGI_ERROR_WAIT_TIMEOUT = unchecked((int)0x887A0027);
        private const int DXGI_ERROR_ACCESS_LOST = unchecked((int)0x887A0026);
        private const int DXGI_ERROR_INVALID_CALL = unchecked((int)0x887A0001);

        [StructLayout(LayoutKind.Sequential)]
        public struct DXGI_OUTDUPL_FRAME_INFO
        {
            public long LastPresentTime;
            public long LastMouseUpdateTime;
            public uint AccumulatedFrames;
            public bool RectsCoalesced;
            public bool ProtectedContentMaskedOut;
            public uint PointerPosition_Visible;
            public Win32Native.POINT PointerPosition_Position;
            public uint TotalMetadataBufferSize;
            public uint PointerShapeBufferSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DXGI_OUTDUPL_DESC
        {
            public int ModeDesc_Width;
            public int ModeDesc_Height;
            public int ModeDesc_RefreshRate_Numerator;
            public int ModeDesc_RefreshRate_Denominator;
            public int ModeDesc_Format;
            public int ModeDesc_ScanlineOrdering;
            public int ModeDesc_Scaling;
            public int Rotation;
            public int DesktopImageInSystemMemory;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct D3D11_TEXTURE2D_DESC
        {
            public uint Width;
            public uint Height;
            public uint MipLevels;
            public uint ArraySize;
            public int Format;
            public int SampleDesc_Count;
            public int SampleDesc_Quality;
            public int Usage;
            public uint BindFlags;
            public uint CPUAccessFlags;
            public uint MiscFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct D3D11_MAPPED_SUBRESOURCE
        {
            public IntPtr pData;
            public uint RowPitch;
            public uint DepthPitch;
        }

        #endregion

        #region DllImports & COM Prototypes

        [DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int D3D11CreateDevice(
            IntPtr pAdapter,
            int DriverType,
            IntPtr Software,
            uint Flags,
            IntPtr[]? pFeatureLevels,
            uint FeatureLevels,
            uint SDKVersion,
            out IntPtr ppDevice,
            out int pFeatureLevel,
            out IntPtr ppImmediateContext);

        [ComImport]
        [Guid("db6f6ddb-ac77-4e88-8253-819df9bbf140")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ID3D11Device
        {
            [PreserveSig] int CreateBuffer(IntPtr pDesc, IntPtr pInitialData, out IntPtr ppBuffer);
            [PreserveSig] int CreateTexture1D(IntPtr pDesc, IntPtr pInitialData, out IntPtr ppTexture1D);
            [PreserveSig] int CreateTexture2D(ref D3D11_TEXTURE2D_DESC pDesc, IntPtr pInitialData, out IntPtr ppTexture2D);
        }

        [ComImport]
        [Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ID3D11DeviceContext
        {
            [PreserveSig] void GetDevice(out IntPtr ppDevice);
            [PreserveSig] void GetPrivateData();
            [PreserveSig] void SetPrivateData();
            [PreserveSig] void SetPrivateDataInterface();
            [PreserveSig] void VSSetConstantBuffers();
            [PreserveSig] void PSSetShaderResources();
            [PreserveSig] void PSSetShader();
            [PreserveSig] void PSSetSamplers();
            [PreserveSig] void VSSetShader();
            [PreserveSig] void DrawIndexed();
            [PreserveSig] void Draw();
            [PreserveSig] int Map(IntPtr pResource, uint Subresource, int MapType, uint MapFlags, out D3D11_MAPPED_SUBRESOURCE pMappedResource);
            [PreserveSig] void Unmap(IntPtr pResource, uint Subresource);
            [PreserveSig] void PSSetConstantBuffers();
            [PreserveSig] void IASetInputLayout();
            [PreserveSig] void IASetVertexBuffers();
            [PreserveSig] void IASetIndexBuffer();
            [PreserveSig] void DrawIndexedInstanced();
            [PreserveSig] void DrawInstanced();
            [PreserveSig] void GSSetConstantBuffers();
            [PreserveSig] void GSSetShader();
            [PreserveSig] void IASetPrimitiveTopology();
            [PreserveSig] void CSSetShaderResources();
            [PreserveSig] void CSSetUnorderedAccessViews();
            [PreserveSig] void CSSetShader();
            [PreserveSig] void CSSetSamplers();
            [PreserveSig] void CSSetConstantBuffers();
            [PreserveSig] void VSSetShaderResources();
            [PreserveSig] void VSSetSamplers();
            [PreserveSig] void Begin();
            [PreserveSig] void End();
            [PreserveSig] void GetData();
            [PreserveSig] void SetPredication();
            [PreserveSig] void GSSetShaderResources();
            [PreserveSig] void GSSetSamplers();
            [PreserveSig] void OMSetRenderTargets();
            [PreserveSig] void OMSetRenderTargetsAndUnorderedAccessViews();
            [PreserveSig] void OMSetBlendState();
            [PreserveSig] void OMSetDepthStencilState();
            [PreserveSig] void SOSetTargets();
            [PreserveSig] void DrawAuto();
            [PreserveSig] void DrawIndexedInstancedIndirect();
            [PreserveSig] void DrawInstancedIndirect();
            [PreserveSig] void Dispatch();
            [PreserveSig] void DispatchIndirect();
            [PreserveSig] void RSSetState();
            [PreserveSig] void RSSetViewports();
            [PreserveSig] void RSSetScissorRects();
            [PreserveSig] void CopySubresourceRegion();
            [PreserveSig] void CopyResource(IntPtr pDstResource, IntPtr pSrcResource);
        }

        [ComImport]
        [Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDXGIDevice
        {
            [PreserveSig] int SetPrivateData();
            [PreserveSig] int SetPrivateDataInterface();
            [PreserveSig] int GetPrivateData();
            [PreserveSig] int GetParent();
            [PreserveSig] int GetAdapter(out IntPtr pAdapter);
        }

        [ComImport]
        [Guid("2411e3bf-fbdd-4237-9777-62425cb97d4c")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDXGIAdapter
        {
            [PreserveSig] int SetPrivateData();
            [PreserveSig] int SetPrivateDataInterface();
            [PreserveSig] int GetPrivateData();
            [PreserveSig] int GetParent();
            [PreserveSig] int EnumOutputs(uint Output, out IntPtr ppOutput);
        }

        [ComImport]
        [Guid("00ab88a3-4e05-4b3e-b882-9e012a2e1d70")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDXGIOutput1
        {
            // IDXGIObject
            [PreserveSig] int SetPrivateData();
            [PreserveSig] int SetPrivateDataInterface();
            [PreserveSig] int GetPrivateData();
            [PreserveSig] int GetParent();
            // IDXGIOutput
            [PreserveSig] int GetDesc(IntPtr pDesc);
            [PreserveSig] int GetDisplayModeList();
            [PreserveSig] int FindClosestMatchingMode();
            [PreserveSig] int WaitForVBlank();
            [PreserveSig] int TakeOwnership();
            [PreserveSig] void ReleaseOwnership();
            [PreserveSig] int GetGammaControlCapabilities();
            [PreserveSig] int SetGammaControl();
            [PreserveSig] int GetGammaControl();
            [PreserveSig] int SetDisplaySurface();
            [PreserveSig] int GetDisplaySurfaceData();
            [PreserveSig] int GetFrameStatistics();
            // IDXGIOutput1
            [PreserveSig] int GetDisplayModeList1();
            [PreserveSig] int FindClosestMatchingMode1();
            [PreserveSig] int GetDisplaySurfaceData1();
            [PreserveSig] int DuplicateOutput([MarshalAs(UnmanagedType.IUnknown)] object pDevice, out IntPtr ppOutputDuplication);
        }

        [ComImport]
        [Guid("191cfac3-a39c-4720-b69e-e489e47a12ce")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDXGIOutputDuplication
        {
            // IDXGIObject
            [PreserveSig] int SetPrivateData();
            [PreserveSig] int SetPrivateDataInterface();
            [PreserveSig] int GetPrivateData();
            [PreserveSig] int GetParent();
            // IDXGIOutputDuplication
            [PreserveSig] void GetDesc(out DXGI_OUTDUPL_DESC pDesc);
            [PreserveSig] int AcquireNextFrame(uint TimeoutInMilliseconds, out DXGI_OUTDUPL_FRAME_INFO pFrameInfo, out IntPtr ppDesktopResource);
            [PreserveSig] int GetFrameDirtyRects();
            [PreserveSig] int GetFrameMoveRects();
            [PreserveSig] int GetFramePointerShape();
            [PreserveSig] int MapDesktopSurface();
            [PreserveSig] int UnmapDesktopSurface();
            [PreserveSig] int ReleaseFrame();
        }

        #endregion

        #region Singleton & State

        private static readonly object _syncLock = new object();
        private static DxgiDesktopDuplicator? _instance;

        public static DxgiDesktopDuplicator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncLock)
                    {
                        _instance ??= new DxgiDesktopDuplicator();
                    }
                }
                return _instance;
            }
        }

        private IntPtr _pDevice = IntPtr.Zero;
        private IntPtr _pDeviceContext = IntPtr.Zero;
        private IntPtr _pOutputDuplication = IntPtr.Zero;
        private IntPtr _pStagingTexture = IntPtr.Zero;

        private ID3D11Device? _d3d11Device;
        private ID3D11DeviceContext? _d3d11Context;
        private IDXGIOutputDuplication? _outputDuplication;

        private int _screenWidth;
        private int _screenHeight;
        private byte[]? _lastFrameBuffer;
        private bool _isInitialized;

        public int ScreenWidth => _screenWidth;
        public int ScreenHeight => _screenHeight;
        public bool IsInitialized => _isInitialized;

        #endregion

        public DxgiDesktopDuplicator()
        {
            Initialize();
        }

        /// <summary>
        /// DXGI D3D11 cihazını ve Desktop Duplication arayüzünü ilklendirir.
        /// </summary>
        public bool Initialize(uint outputIndex = 0)
        {
            lock (_syncLock)
            {
                Cleanup();

                try
                {
                    int hr = D3D11CreateDevice(
                        IntPtr.Zero,
                        D3D_DRIVER_TYPE_HARDWARE,
                        IntPtr.Zero,
                        D3D11_CREATE_DEVICE_BGRA_SUPPORT,
                        null,
                        0,
                        D3D11_SDK_VERSION,
                        out _pDevice,
                        out _,
                        out _pDeviceContext);

                    if (hr < 0 || _pDevice == IntPtr.Zero || _pDeviceContext == IntPtr.Zero)
                    {
                        return false;
                    }

                    _d3d11Device = (ID3D11Device)Marshal.GetObjectForIUnknown(_pDevice);
                    _d3d11Context = (ID3D11DeviceContext)Marshal.GetObjectForIUnknown(_pDeviceContext);

                    // Query IDXGIDevice
                    IntPtr pDxgiDevicePtr;
                    hr = Marshal.QueryInterface(_pDevice, in IID_IDXGIDevice, out pDxgiDevicePtr);
                    if (hr < 0 || pDxgiDevicePtr == IntPtr.Zero) return false;

                    IDXGIDevice dxgiDevice = (IDXGIDevice)Marshal.GetObjectForIUnknown(pDxgiDevicePtr);
                    dxgiDevice.GetAdapter(out IntPtr pAdapter);
                    Marshal.Release(pDxgiDevicePtr);

                    if (pAdapter == IntPtr.Zero) return false;

                    IDXGIAdapter adapter = (IDXGIAdapter)Marshal.GetObjectForIUnknown(pAdapter);
                    hr = adapter.EnumOutputs(outputIndex, out IntPtr pOutput);
                    Marshal.Release(pAdapter);

                    if (hr < 0 || pOutput == IntPtr.Zero) return false;

                    hr = Marshal.QueryInterface(pOutput, in IID_IDXGIOutput1, out IntPtr pOutput1Ptr);
                    Marshal.Release(pOutput);

                    if (hr < 0 || pOutput1Ptr == IntPtr.Zero) return false;

                    IDXGIOutput1 output1 = (IDXGIOutput1)Marshal.GetObjectForIUnknown(pOutput1Ptr);
                    hr = output1.DuplicateOutput(_d3d11Device, out _pOutputDuplication);
                    Marshal.Release(pOutput1Ptr);

                    if (hr < 0 || _pOutputDuplication == IntPtr.Zero) return false;

                    _outputDuplication = (IDXGIOutputDuplication)Marshal.GetObjectForIUnknown(_pOutputDuplication);

                    _outputDuplication.GetDesc(out DXGI_OUTDUPL_DESC desc);
                    _screenWidth = desc.ModeDesc_Width;
                    _screenHeight = desc.ModeDesc_Height;

                    if (_screenWidth <= 0 || _screenHeight <= 0)
                    {
                        _screenWidth = Screen.PrimaryScreen?.Bounds.Width ?? 1920;
                        _screenHeight = Screen.PrimaryScreen?.Bounds.Height ?? 1080;
                    }

                    // CPU okunabilir Staging Texture oluştur
                    D3D11_TEXTURE2D_DESC stagingDesc = new D3D11_TEXTURE2D_DESC
                    {
                        Width = (uint)_screenWidth,
                        Height = (uint)_screenHeight,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = DXGI_FORMAT_B8G8R8A8_UNORM,
                        SampleDesc_Count = 1,
                        SampleDesc_Quality = 0,
                        Usage = D3D11_USAGE_STAGING,
                        BindFlags = 0,
                        CPUAccessFlags = D3D11_CPU_ACCESS_READ,
                        MiscFlags = 0
                    };

                    hr = _d3d11Device.CreateTexture2D(ref stagingDesc, IntPtr.Zero, out _pStagingTexture);
                    if (hr < 0 || _pStagingTexture == IntPtr.Zero) return false;

                    _lastFrameBuffer = new byte[_screenWidth * _screenHeight * 4];
                    _isInitialized = true;
                    return true;
                }
                catch
                {
                    Cleanup();
                    return false;
                }
            }
        }

        /// <summary>
        /// GPU Desktop Duplication ile masaüstünün en güncel tam karesini yakalar.
        /// </summary>
        /// <param name="timeoutMs">Kare bekleme zaman aşımı (ms)</param>
        /// <returns>Tüm ekran Bitmap görseli veya null</returns>
        public Bitmap? CaptureDesktop(uint timeoutMs = 25)
        {
            lock (_syncLock)
            {
                if (!_isInitialized && !Initialize())
                {
                    return null;
                }

                if (_outputDuplication == null || _d3d11Context == null || _pStagingTexture == IntPtr.Zero)
                {
                    return null;
                }

                IntPtr pDesktopResource = IntPtr.Zero;
                bool frameAcquired = false;

                try
                {
                    int hr = _outputDuplication.AcquireNextFrame(timeoutMs, out _, out pDesktopResource);

                    if (hr == DXGI_ERROR_ACCESS_LOST)
                    {
                        // Çözünürlük veya tam ekran geçişi nedeniyle erişim koptu, yeniden ilklendir
                        Initialize();
                        return null;
                    }

                    if (hr == 0 && pDesktopResource != IntPtr.Zero)
                    {
                        frameAcquired = true;

                        // Desktop kaynağını staging texture'a kopyala
                        _d3d11Context.CopyResource(_pStagingTexture, pDesktopResource);

                        // Texture'ı CPU belleğine Map et
                        int mapHr = _d3d11Context.Map(_pStagingTexture, 0, D3D11_MAP_READ, 0, out D3D11_MAPPED_SUBRESOURCE mapped);
                        if (mapHr == 0 && mapped.pData != IntPtr.Zero)
                        {
                            try
                            {
                                Bitmap bmp = new Bitmap(_screenWidth, _screenHeight, PixelFormat.Format32bppArgb);
                                BitmapData bmpData = bmp.LockBits(
                                    new Rectangle(0, 0, _screenWidth, _screenHeight),
                                    ImageLockMode.WriteOnly,
                                    PixelFormat.Format32bppArgb);

                                unsafe
                                {
                                    byte* srcPtr = (byte*)mapped.pData.ToPointer();
                                    byte* dstPtr = (byte*)bmpData.Scan0.ToPointer();
                                    int rowBytes = _screenWidth * 4;

                                    for (int y = 0; y < _screenHeight; y++)
                                    {
                                        Buffer.MemoryCopy(
                                            srcPtr + (y * mapped.RowPitch),
                                            dstPtr + (y * bmpData.Stride),
                                            rowBytes,
                                            rowBytes);
                                    }

                                    // Son başarılı kareyi tamponda da sakla
                                    if (_lastFrameBuffer != null)
                                    {
                                        Marshal.Copy(bmpData.Scan0, _lastFrameBuffer, 0, _lastFrameBuffer.Length);
                                    }
                                }

                                bmp.UnlockBits(bmpData);
                                return bmp;
                            }
                            finally
                            {
                                _d3d11Context.Unmap(_pStagingTexture, 0);
                            }
                        }
                    }
                    else if (hr == DXGI_ERROR_WAIT_TIMEOUT && _lastFrameBuffer != null && _lastFrameBuffer.Length > 0)
                    {
                        // Ekranda değişiklik yoksa son başarılı tampondan Bitmap üret
                        Bitmap cachedBmp = new Bitmap(_screenWidth, _screenHeight, PixelFormat.Format32bppArgb);
                        BitmapData bmpData = cachedBmp.LockBits(
                            new Rectangle(0, 0, _screenWidth, _screenHeight),
                            ImageLockMode.WriteOnly,
                            PixelFormat.Format32bppArgb);

                        Marshal.Copy(_lastFrameBuffer, 0, bmpData.Scan0, _lastFrameBuffer.Length);
                        cachedBmp.UnlockBits(bmpData);
                        return cachedBmp;
                    }
                }
                catch
                {
                    return null;
                }
                finally
                {
                    if (frameAcquired && _outputDuplication != null)
                    {
                        try { _outputDuplication.ReleaseFrame(); } catch { }
                    }

                    if (pDesktopResource != IntPtr.Zero)
                    {
                        Marshal.Release(pDesktopResource);
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Masaüstü görüntüsünden doğrudan belirtilen ekran koordinatlarındaki dikdörtgeni kırpar (Crop).
        /// </summary>
        /// <param name="screenCropRect">Masaüstü ekran koordinatlarında kırpılacak alan</param>
        /// <param name="timeoutMs">Zaman aşımı</param>
        /// <returns>Kırpılmış Bitmap veya null</returns>
        public Bitmap? CaptureScreenRegion(Rectangle screenCropRect, uint timeoutMs = 25)
        {
            if (screenCropRect.Width <= 0 || screenCropRect.Height <= 0)
            {
                return null;
            }

            using (Bitmap? fullDesktop = CaptureDesktop(timeoutMs))
            {
                if (fullDesktop == null)
                {
                    return null;
                }

                int cropX = Math.Clamp(screenCropRect.X, 0, fullDesktop.Width - 1);
                int cropY = Math.Clamp(screenCropRect.Y, 0, fullDesktop.Height - 1);
                int cropW = Math.Min(screenCropRect.Width, fullDesktop.Width - cropX);
                int cropH = Math.Min(screenCropRect.Height, fullDesktop.Height - cropY);

                if (cropW <= 0 || cropH <= 0)
                {
                    return null;
                }

                Rectangle safeRect = new Rectangle(cropX, cropY, cropW, cropH);
                return fullDesktop.Clone(safeRect, PixelFormat.Format32bppArgb);
            }
        }

        private void Cleanup()
        {
            _isInitialized = false;

            if (_pStagingTexture != IntPtr.Zero)
            {
                Marshal.Release(_pStagingTexture);
                _pStagingTexture = IntPtr.Zero;
            }

            if (_pOutputDuplication != IntPtr.Zero)
            {
                Marshal.Release(_pOutputDuplication);
                _pOutputDuplication = IntPtr.Zero;
            }

            if (_pDeviceContext != IntPtr.Zero)
            {
                Marshal.Release(_pDeviceContext);
                _pDeviceContext = IntPtr.Zero;
            }

            if (_pDevice != IntPtr.Zero)
            {
                Marshal.Release(_pDevice);
                _pDevice = IntPtr.Zero;
            }

            _d3d11Device = null;
            _d3d11Context = null;
            _outputDuplication = null;
        }

        public void Dispose()
        {
            lock (_syncLock)
            {
                Cleanup();
            }
            GC.SuppressFinalize(this);
        }
    }
}
