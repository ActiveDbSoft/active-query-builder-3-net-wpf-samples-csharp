//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2022 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MSDASC
{
    [ComConversionLoss]
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct _COSERVERINFO
    {
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszName;
        [ComConversionLoss]
        public IntPtr pAuthInfo;
        public uint dwReserved2;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct __MIDL_IWinTypes_0009
    {
        [FieldOffset(0)]
        public int hInproc;
        [FieldOffset(0)]
        public int hRemote;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct _RemotableHandle
    {
        public int fContext;
        public __MIDL_IWinTypes_0009 u;
    }

    [Guid("2206CCB1-19C1-11D1-89E0-00C04FD7A829")]
    [InterfaceType(1)]
    [ComConversionLoss]
    [ComImport]
    public interface IDataInitialize
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetDataSource(
          [MarshalAs(UnmanagedType.IUnknown), In] object pUnkOuter,
          [In] uint dwClsCtx,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszInitializationString,
          [In] ref Guid riid,
          [MarshalAs(UnmanagedType.IUnknown), In, Out] ref object ppDataSource);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetInitializationString(
          [MarshalAs(UnmanagedType.IUnknown), In] object pDataSource,
          [In] sbyte fIncludePassword,
          [MarshalAs(UnmanagedType.LPWStr)] out string ppwszInitString);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void CreateDBInstance(
          [In] ref Guid clsidProvider,
          [MarshalAs(UnmanagedType.IUnknown), In] object pUnkOuter,
          [In] uint dwClsCtx,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszReserved,
          [In] ref Guid riid,
          [MarshalAs(UnmanagedType.IUnknown)] out object ppDataSource);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RemoteCreateDBInstanceEx(
          [In] ref Guid clsidProvider,
          [MarshalAs(UnmanagedType.IUnknown), In] object pUnkOuter,
          [In] uint dwClsCtx,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszReserved,
          [In] ref _COSERVERINFO pServerInfo,
          [In] uint cmq,
          [In] IntPtr rgpIID,
          [MarshalAs(UnmanagedType.IUnknown)] out object rgpItf,
          [MarshalAs(UnmanagedType.Error)] out int rghr);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void LoadStringFromStorage([MarshalAs(UnmanagedType.LPWStr), In] string pwszFileName, [MarshalAs(UnmanagedType.LPWStr)] out string ppwszInitializationString);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void WriteStringToStorage(
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszFileName,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszInitializationString,
          [In] uint dwCreationDisposition);
    }

    [InterfaceType(1)]
    [TypeLibType(512)]
    [Guid("2206CCB0-19C1-11D1-89E0-00C04FD7A829")]
    [ComImport]
    public interface IDBPromptInitialize
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void PromptDataSource(
            [MarshalAs(UnmanagedType.IUnknown), In] object pUnkOuter,
            [ComAliasName("MSDASC.wireHWND"), In] ref _RemotableHandle hWndParent,
            [In] uint dwPromptOptions,
            [In] uint cSourceTypeFilter,
            [In] ref uint rgSourceTypeFilter,
            [MarshalAs(UnmanagedType.LPWStr), In] string pwszszzProviderFilter,
            [In] ref Guid riid,
            [MarshalAs(UnmanagedType.IUnknown), In, Out] ref object ppDataSource);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void PromptFileName(
            [ComAliasName("MSDASC.wireHWND"), In] ref _RemotableHandle hWndParent,
            [In] uint dwPromptOptions,
            [MarshalAs(UnmanagedType.LPWStr), In] string pwszInitialDirectory,
            [MarshalAs(UnmanagedType.LPWStr), In] string pwszInitialFile,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszSelectedFile);
    }

    [TypeLibType(4160)]
    [Guid("2206CCB2-19C1-11D1-89E0-00C04FD7A829")]
    [ComImport]
    public interface IDataSourceLocator
    {
        [DispId(1610743808)]
        int hWnd { [DispId(1610743808), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; [DispId(1610743808), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [param: In] set; }

        [DispId(1610743810)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        object PromptNew();

        [DispId(1610743811)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        bool PromptEdit([MarshalAs(UnmanagedType.IDispatch), In, Out] ref object ppADOConnection);
    }

    [CoClass(typeof(DataLinksClass))]
    [Guid("2206CCB2-19C1-11D1-89E0-00C04FD7A829")]
    [ComImport]
    public interface DataLinks : IDataSourceLocator
    {
    }

    [TypeLibType(2)]
    //[ClassInterface(0)]
    [ComConversionLoss]
    [Guid("2206CDB2-19C1-11D1-89E0-00C04FD7A829")]
    [ComImport]
    public class DataLinksClass : IDataSourceLocator, DataLinks, IDBPromptInitialize, IDataInitialize
    {
        //[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        //public extern DataLinksClass();

        [DispId(1610743808)]
        public virtual extern int hWnd { [DispId(1610743808), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; [DispId(1610743808), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [param: In] set; }

        [DispId(1610743810)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        public virtual extern object PromptNew();

        [DispId(1610743811)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern bool PromptEdit([MarshalAs(UnmanagedType.IDispatch), In, Out] ref object ppADOConnection);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void PromptDataSource(
          [MarshalAs(UnmanagedType.IUnknown), In] object pUnkOuter,
          [ComAliasName("MSDASC.wireHWND"), In] ref _RemotableHandle hWndParent,
          [In] uint dwPromptOptions,
          [In] uint cSourceTypeFilter,
          [In] ref uint rgSourceTypeFilter,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszszzProviderFilter,
          [In] ref Guid riid,
          [MarshalAs(UnmanagedType.IUnknown), In, Out] ref object ppDataSource);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void PromptFileName(
          [ComAliasName("MSDASC.wireHWND"), In] ref _RemotableHandle hWndParent,
          [In] uint dwPromptOptions,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszInitialDirectory,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszInitialFile,
          [MarshalAs(UnmanagedType.LPWStr)] out string ppwszSelectedFile);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void GetDataSource(
          [MarshalAs(UnmanagedType.IUnknown), In] object pUnkOuter,
          [In] uint dwClsCtx,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszInitializationString,
          [In] ref Guid riid,
          [MarshalAs(UnmanagedType.IUnknown), In, Out] ref object ppDataSource);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void GetInitializationString(
          [MarshalAs(UnmanagedType.IUnknown), In] object pDataSource,
          [In] sbyte fIncludePassword,
          [MarshalAs(UnmanagedType.LPWStr)] out string ppwszInitString);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void CreateDBInstance(
          [In] ref Guid clsidProvider,
          [MarshalAs(UnmanagedType.IUnknown), In] object pUnkOuter,
          [In] uint dwClsCtx,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszReserved,
          [In] ref Guid riid,
          [MarshalAs(UnmanagedType.IUnknown)] out object ppDataSource);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void RemoteCreateDBInstanceEx(
          [In] ref Guid clsidProvider,
          [MarshalAs(UnmanagedType.IUnknown), In] object pUnkOuter,
          [In] uint dwClsCtx,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszReserved,
          [In] ref _COSERVERINFO pServerInfo,
          [In] uint cmq,
          [In] IntPtr rgpIID,
          [MarshalAs(UnmanagedType.IUnknown)] out object rgpItf,
          [MarshalAs(UnmanagedType.Error)] out int rghr);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void LoadStringFromStorage(
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszFileName,
          [MarshalAs(UnmanagedType.LPWStr)] out string ppwszInitializationString);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void WriteStringToStorage(
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszFileName,
          [MarshalAs(UnmanagedType.LPWStr), In] string pwszInitializationString,
          [In] uint dwCreationDisposition);
    }
}
