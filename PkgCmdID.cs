// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace LandOfJoe.NuspecPackager
{
    static class PkgCmdIDList
    {
        public const uint cmdidMyCommand = 0x100;
        public const uint cmdidPackageFromProject = 0x102;
        public const uint cmdidPackageSymbols = 0x101;
    };
}