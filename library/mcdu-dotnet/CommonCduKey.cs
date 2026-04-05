// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace McduDotNet
{
    /// <summary>
    /// An enumeration of keys that the CDUs have in common, or are *very* roughly
    /// equivalent (even if it's just in name and/or location on the keyboard).
    /// </summary>
    public enum CommonCduKey
    {
        DeviceSpecific = 1000,

        LineSelectLeft1 = CduKey.LineSelectLeft1,
        LineSelectLeft2 = CduKey.LineSelectLeft2,
        LineSelectLeft3 = CduKey.LineSelectLeft3,
        LineSelectLeft4 = CduKey.LineSelectLeft4,
        LineSelectLeft5 = CduKey.LineSelectLeft5,
        LineSelectLeft6 = CduKey.LineSelectLeft6,
        LineSelectRight1 = CduKey.LineSelectRight1,
        LineSelectRight2 = CduKey.LineSelectRight2,
        LineSelectRight3 = CduKey.LineSelectRight3,
        LineSelectRight4 = CduKey.LineSelectRight4,
        LineSelectRight5 = CduKey.LineSelectRight5,
        LineSelectRight6 = CduKey.LineSelectRight6,

        Digit1 = CduKey.Digit1,
        Digit2 = CduKey.Digit2,
        Digit3 = CduKey.Digit3,
        Digit4 = CduKey.Digit4,
        Digit5 = CduKey.Digit5,
        Digit6 = CduKey.Digit6,
        Digit7 = CduKey.Digit7,
        Digit8 = CduKey.Digit8,
        Digit9 = CduKey.Digit9,
        DecimalPoint = CduKey.DecimalPoint,
        Digit0 = CduKey.Digit0,
        PositiveNegative = CduKey.PositiveNegative,
        A = CduKey.A,
        B = CduKey.B,
        C = CduKey.C,
        D = CduKey.D,
        E = CduKey.E,
        F = CduKey.F,
        G = CduKey.G,
        H = CduKey.H,
        I = CduKey.I,
        J = CduKey.J,
        K = CduKey.K,
        L = CduKey.L,
        M = CduKey.M,
        N = CduKey.N,
        O = CduKey.O,
        P = CduKey.P,
        Q = CduKey.Q,
        R = CduKey.R,
        S = CduKey.S,
        T = CduKey.T,
        U = CduKey.U,
        V = CduKey.V,
        W = CduKey.W,
        X = CduKey.X,
        Y = CduKey.Y,
        Z = CduKey.Z,
        Slash = CduKey.Slash,
        Space = CduKey.Space,
        Clr = CduKey.Clr,

        Brt = CduKey.Brt,
        Dim = CduKey.Dim,
        Prog = CduKey.Prog,

        EitherOr =                  2000,
        InitOrInitRef =             EitherOr + 1,
        McduMenuOrMenu =            EitherOr + 2,
        FPlnOrRte =                 EitherOr + 3,
        AirportOrDepArr =           EitherOr + 4,
        RadNavOrNavRad =            EitherOr + 5,
        RightArrowOrNextPage =      EitherOr + 6,
        LeftArrowOrPrevPage =       EitherOr + 7,
        SecFPlnOrAltn =             EitherOr + 8,
        OvfyOrDel =                 EitherOr + 9,
        AtcCommOrFmcComm =          EitherOr + 10,

        // unmapped:
        // Legs,        PFP-*
        // Hold,        PFP-*
        // Exec,        PFP-*
        // Fix,         PFP-*
        // Clb,         PFP-3N
        // Crz,         PFP-3N
        // Des,         PFP-3N
        // N1Limit,     PFP-3N
        // Data,        MCDU
        // Dir,         MCDU
        // FuelPred,    MCDU
        // Perf,        MCDU
        // UpArrow,     MCDU
        // DownArrow,   MCDU
        // VNav,        PFP-7
    }
}
