using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Zan.Services;

/// <summary>
/// Thin wrapper over the Windows Credential Manager (advapi32 Cred* APIs) for
/// generic secrets. No NuGet dependency. Secrets are stored in the user's
/// credential vault, never written to disk in plaintext by us.
/// </summary>
internal static class CredentialStore
{
    private const int CRED_TYPE_GENERIC = 1;
    private const int CRED_PERSIST_LOCAL_MACHINE = 2;

    public static void Save(string target, string userName, string secret)
    {
        var blob = Encoding.Unicode.GetBytes(secret);
        var blobPtr = Marshal.AllocHGlobal(blob.Length);
        Marshal.Copy(blob, 0, blobPtr, blob.Length);
        try
        {
            var cred = new CREDENTIAL
            {
                Type = CRED_TYPE_GENERIC,
                TargetName = target,
                CredentialBlobSize = blob.Length,
                CredentialBlob = blobPtr,
                Persist = CRED_PERSIST_LOCAL_MACHINE,
                UserName = userName,
            };
            if (!CredWriteW(ref cred, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "CredWrite failed");
        }
        finally
        {
            Marshal.FreeHGlobal(blobPtr);
        }
    }

    public static string? Read(string target)
    {
        if (!CredReadW(target, CRED_TYPE_GENERIC, 0, out var ptr))
            return null;
        try
        {
            var cred = Marshal.PtrToStructure<CREDENTIAL>(ptr);
            if (cred.CredentialBlobSize == 0 || cred.CredentialBlob == IntPtr.Zero)
                return string.Empty;
            var bytes = new byte[cred.CredentialBlobSize];
            Marshal.Copy(cred.CredentialBlob, bytes, 0, cred.CredentialBlobSize);
            return Encoding.Unicode.GetString(bytes);
        }
        finally
        {
            CredFree(ptr);
        }
    }

    public static void Delete(string target)
    {
        // Ignore "not found"; deleting a missing credential is a no-op for us.
        CredDeleteW(target, CRED_TYPE_GENERIC, 0);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public int Flags;
        public int Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string? UserName;
    }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWriteW(ref CREDENTIAL credential, int flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredReadW(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDeleteW(string target, int type, int flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree(IntPtr cred);
}
