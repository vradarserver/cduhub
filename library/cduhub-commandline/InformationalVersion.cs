// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Cduhub.CommandLine
{
    /// <summary>
    /// The version number information extracted from an assembly's InformationalVersion attribute.
    /// </summary>
    /// <remarks>
    /// <para>The informational version is assumed to be in one of three forms:</para>
    /// <para>Normal release: major.minor.patch</para>
    /// <para>Alpha release: major.minor.patch-alpha-revision</para>
    /// <para>Beta release: major.minor.patch-beta-revision</para>
    /// <para>
    /// NuGet does not recognise revision numbers as numbers for sorting so you should use leading zeros with
    /// them. You do not need leading zeros on major, minor or patch, NuGet recognises those.
    /// </para>
    /// <para>
    /// .NET compilers will often append the git hash for the commit that the source was built from in the
    /// form '+HASH'. The parser will optionally extract that and record it in the <see cref="CommitHash"/>
    /// property.
    /// </para>
    /// </remarks>
    public class InformationalVersion : IComparable<InformationalVersion>
    {
        static Regex _ParseRegex = new Regex(
            @"(?<major>\d+).(?<minor>\d+).(?<patch>\d+)(-(?<revisionType>alpha|beta)-(?<revision>\d+))?(\+(?<commitHash>.+))?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// The content of the Version tag in an attribute, as read via an assembly's InformationalVersion
        /// attribute.
        /// </summary>
        public string VersionTag { get; } = "";

        /// <summary>
        /// The type of release parsed from <see cref="VersionTag"/>.
        /// </summary>
        public ReleaseType ReleaseType { get; }

        /// <summary>
        /// The major version number parsed from <see cref="VersionTag"/>.
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// The minor version number parsed from <see cref="VersionTag"/>.
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// The patch version number parsed from <see cref="VersionTag"/>.
        /// </summary>
        public int Patch { get; }

        /// <summary>
        /// The alpha or beta revision number parsed from <see cref="VersionTag"/>.
        /// </summary>
        public int Revision { get; }

        /// <summary>
        /// The optional version control identifier that identifies the version of the source that the
        /// application was built from. Parsed from <see cref="VersionTag"/>.
        /// </summary>
        public string CommitHash { get; } = "";

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public InformationalVersion()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="versionTag"></param>
        /// <param name="releaseType"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        /// <param name="revision"></param>
        /// <param name="commitHash"></param>
        public InformationalVersion(
            string versionTag,
            ReleaseType releaseType,
            int major,
            int minor,
            int patch,
            int revision,
            string commitHash
        ) : this()
        {
            VersionTag = versionTag;
            ReleaseType = releaseType;
            Major = major;
            Minor = minor;
            Patch = patch;
            Revision = revision;
            CommitHash = commitHash;
        }

        /// <inheritdoc/>
        public override string ToString() => ReleaseType == ReleaseType.Stable
            ? $"{Major}.{Minor}.{Patch}"
            : $"{Major}.{Minor}.{Patch}-{(ReleaseType == ReleaseType.Alpha ? "alpha" : "beta")}-{Revision}";

        /// <inheritdoc/>
        public int CompareTo(InformationalVersion other)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }

            var result = Major - other.Major;

            if(result == 0) {
                result = Minor - other.Minor;
            }
            if(result == 0) {
                result = Patch - other.Patch;
            }
            if(result == 0) {
                result = (int)ReleaseType - (int)other.ReleaseType;
            }
            if(result == 0) {
                result = Revision - other.Revision;
            }

            return result;
        }

        /// <summary>
        /// Returns true if this version is earlier than the other version, taking into consideration whether
        /// the newer version is alpha or beta and we're stable, alpha or beta.
        /// </summary>
        /// <param name="other">The version to test against</param>
        /// <param name="allowUpdateToAlpha">
        /// True if we want to permit updates from stable or beta to a new alpha.
        /// </param>
        /// <param name="allowUpdateToBeta">
        /// True if we want to permit updates from stable to a new beta.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// <paramref name="allowUpdateToAlpha"/> is always true if the current version is an alpha release.
        /// <paramref name="allowUpdateToBeta"/> is always true if the current version is an alpha or beta
        /// release.
        /// </remarks>
        public bool CanUpdateTo(InformationalVersion other, bool allowUpdateToAlpha, bool allowUpdateToBeta)
        {
            if(other == null) {
                throw new ArgumentNullException(nameof(other));
            }

            allowUpdateToAlpha = allowUpdateToAlpha || ReleaseType == ReleaseType.Alpha;
            allowUpdateToBeta =  allowUpdateToBeta  || ReleaseType != ReleaseType.Stable;

            var result = (other.ReleaseType != ReleaseType.Alpha || allowUpdateToAlpha)
                      && (other.ReleaseType != ReleaseType.Beta  || allowUpdateToBeta)
                      && CompareTo(other) < 0;

            return result;
        }

        /// <summary>
        /// Parses a version tag into a version object.
        /// </summary>
        /// <param name="versionTag"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static InformationalVersion Parse(string versionTag)
        {
            if(!TryParse(versionTag, out var result)) {
                throw new ArgumentOutOfRangeException($"{versionTag} cannot be parsed into an {nameof(InformationalVersion)}");
            }
            return result;
        }

        /// <summary>
        /// Tries to parse a version tag into a version.
        /// </summary>
        /// <param name="versionTag"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool TryParse(string versionTag, out InformationalVersion version)
        {
            version = default;      // <-- VS2022 cannot figure out that there's no path where version is not assigned, it needs this to compile

            var match = _ParseRegex.Match(versionTag ?? "");
            var result = match.Success;

            if(result) {
                int major, minor = 0, patch = 0, revision = 0;

                result = int.TryParse(match.Groups["major"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out major)
                      && int.TryParse(match.Groups["minor"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out minor)
                      && int.TryParse(match.Groups["patch"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out patch);
                var revisionType = "";
                if(result && match.Groups["revisionType"].Success) {
                    revisionType = match.Groups["revisionType"].Value.ToLower();
                    result = match.Groups["revision"].Success;
                    result = result && int.TryParse(match.Groups["revision"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out revision);
                }
                var commitHash = match.Groups["commitHash"].Value;

                if(result) {
                    version = new InformationalVersion(
                        versionTag,
                        revisionType == "alpha"
                            ? ReleaseType.Alpha
                            : revisionType == "beta"
                                ? ReleaseType.Beta
                                : ReleaseType.Stable,
                        major,
                        minor,
                        patch,
                        revision,
                        commitHash
                    );
                }
            }

            if(!result) {
                version = new InformationalVersion();
            }

            return result;
        }

        /// <summary>
        /// Returns the version information from an assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static InformationalVersion FromAssembly(Assembly assembly)
        {
            InformationalVersion.TryParse(
                assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                out var result
            );

            return result;
        }
    }
}
