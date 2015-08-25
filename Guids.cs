// Guids.cs
// MUST match guids.h
using System;

namespace LandOfJoe.NuspecPackager
{
    static class GuidList
    {
        public const string guidNuspecPackagerPkgString = "cbb25c94-9a4e-4f0e-b8a9-d5195b554e9b";
        public const string guidNuspecPackagerCmdSetString = "44f2fa55-6e66-42d5-8242-226663095298";

        public static readonly Guid guidNuspecPackagerCmdSet = new Guid(guidNuspecPackagerCmdSetString);
    };
}