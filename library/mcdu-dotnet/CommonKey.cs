// Copyright © 2025 onwards, Andrew Whewell
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
    public enum CommonKey
    {
        DeviceSpecific = 1000,

        LineSelectLeft1 = Key.LineSelectLeft1,
        LineSelectLeft2 = Key.LineSelectLeft2,
        LineSelectLeft3 = Key.LineSelectLeft3,
        LineSelectLeft4 = Key.LineSelectLeft4,
        LineSelectLeft5 = Key.LineSelectLeft5,
        LineSelectLeft6 = Key.LineSelectLeft6,
        LineSelectRight1 = Key.LineSelectRight1,
        LineSelectRight2 = Key.LineSelectRight2,
        LineSelectRight3 = Key.LineSelectRight3,
        LineSelectRight4 = Key.LineSelectRight4,
        LineSelectRight5 = Key.LineSelectRight5,
        LineSelectRight6 = Key.LineSelectRight6,

        Digit1 = Key.Digit1,
        Digit2 = Key.Digit2,
        Digit3 = Key.Digit3,
        Digit4 = Key.Digit4,
        Digit5 = Key.Digit5,
        Digit6 = Key.Digit6,
        Digit7 = Key.Digit7,
        Digit8 = Key.Digit8,
        Digit9 = Key.Digit9,
        DecimalPoint = Key.DecimalPoint,
        Digit0 = Key.Digit0,
        PositiveNegative = Key.PositiveNegative,
        A = Key.A,
        B = Key.B,
        C = Key.C,
        D = Key.D,
        E = Key.E,
        F = Key.F,
        G = Key.G,
        H = Key.H,
        I = Key.I,
        J = Key.J,
        K = Key.K,
        L = Key.L,
        M = Key.M,
        N = Key.N,
        O = Key.O,
        P = Key.P,
        Q = Key.Q,
        R = Key.R,
        S = Key.S,
        T = Key.T,
        U = Key.U,
        V = Key.V,
        W = Key.W,
        X = Key.X,
        Y = Key.Y,
        Z = Key.Z,
        Slash = Key.Slash,
        Space = Key.Space,
        Clr = Key.Clr,

        Brt = Key.Brt,
        Dim = Key.Dim,
        Prog = Key.Prog,

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
