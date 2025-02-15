// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// ===================================================================================================
// Portions of the code implemented below are based on the 'Berkeley SoftFloat Release 3e' algorithms.
// ===================================================================================================

/*============================================================
**
**
**
** Purpose: Some floating-point math operations
**
** 
===========================================================*/

//This class contains only static members and doesn't require serialization.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace System
{
    public static partial class Math
    {
        public const double E = 2.7182818284590452354;

        public const double PI = 3.14159265358979323846;

        private const int maxRoundingDigits = 15;

        private const double doubleRoundLimit = 1e16d;

        // This table is required for the Round function which can specify the number of digits to round to
        private static double[] roundPower10Double = new double[] {
          1E0, 1E1, 1E2, 1E3, 1E4, 1E5, 1E6, 1E7, 1E8,
          1E9, 1E10, 1E11, 1E12, 1E13, 1E14, 1E15
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Abs(short value)
        {
            if (value < 0)
            {
                value = (short)-value;
                if (value < 0)
                {
                    ThrowAbsOverflow();
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int value)
        {
            if (value < 0)
            {
                value = -value;
                if (value < 0)
                {
                    ThrowAbsOverflow();
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Abs(long value)
        {
            if (value < 0)
            {
                value = -value;
                if (value < 0)
                {
                    ThrowAbsOverflow();
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static sbyte Abs(sbyte value)
        {
            if (value < 0)
            {
                value = (sbyte)-value;
                if (value < 0)
                {
                    ThrowAbsOverflow();
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Abs(decimal value)
        {
            return decimal.Abs(value);
        }

        [DoesNotReturn]
        [StackTraceHidden]
        private static void ThrowAbsOverflow()
        {
            throw new OverflowException(SR.Overflow_NegateTwosCompNum);
        }

        public static long BigMul(int a, int b)
        {
            return ((long)a) * b;
        }

        public static double BitDecrement(double x)
        {
            var bits = BitConverter.DoubleToInt64Bits(x);

            if (((bits >> 32) & 0x7FF00000) >= 0x7FF00000)
            {
                // NaN returns NaN
                // -Infinity returns -Infinity
                // +Infinity returns double.MaxValue
                return (bits == 0x7FF00000_00000000) ? double.MaxValue : x;
            }

            if (bits == 0x00000000_00000000)
            {
                // +0.0 returns -double.Epsilon
                return -double.Epsilon;
            }

            // Negative values need to be incremented
            // Positive values need to be decremented

            bits += ((bits < 0) ? +1 : -1);
            return BitConverter.Int64BitsToDouble(bits);
        }

        public static double BitIncrement(double x)
        {
            var bits = BitConverter.DoubleToInt64Bits(x);

            if (((bits >> 32) & 0x7FF00000) >= 0x7FF00000)
            {
                // NaN returns NaN
                // -Infinity returns double.MinValue
                // +Infinity returns +Infinity
                return (bits == unchecked((long)(0xFFF00000_00000000))) ? double.MinValue : x;
            }

            if (bits == unchecked((long)(0x80000000_00000000)))
            {
                // -0.0 returns double.Epsilon
                return double.Epsilon;
            }

            // Negative values need to be decremented
            // Positive values need to be incremented

            bits += ((bits < 0) ? -1 : +1);
            return BitConverter.Int64BitsToDouble(bits);
        }

        public static unsafe double CopySign(double x, double y)
        {
            // This method is required to work for all inputs,
            // including NaN, so we operate on the raw bits.

            var xbits = BitConverter.DoubleToInt64Bits(x);
            var ybits = BitConverter.DoubleToInt64Bits(y);

            // If the sign bits of x and y are not the same,
            // flip the sign bit of x and return the new value;
            // otherwise, just return x

            if ((xbits ^ ybits) < 0)
            {
                return BitConverter.Int64BitsToDouble(xbits ^ long.MinValue);
            }

            return x;
        }

        public static int DivRem(int a, int b, out int result)
        {
            // TODO https://github.com/dotnet/coreclr/issues/3439:
            // Restore to using % and / when the JIT is able to eliminate one of the idivs.
            // In the meantime, a * and - is measurably faster than an extra /.

            int div = a / b;
            result = a - (div * b);
            return div;
        }

        public static long DivRem(long a, long b, out long result)
        {
            long div = a / b;
            result = a - (div * b);
            return div;
        }

        internal static uint DivRem(uint a, uint b, out uint result)
        {
            uint div = a / b;
            result = a - (div * b);
            return div;
        }

        internal static ulong DivRem(ulong a, ulong b, out ulong result)
        {
            ulong div = a / b;
            result = a - (div * b);
            return div;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Ceiling(decimal d)
        {
            return decimal.Ceiling(d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp(byte value, byte min, byte max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Clamp(short value, short min, short max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long value, long min, long max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Clamp(sbyte value, sbyte min, sbyte max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Clamp(ushort value, ushort min, ushort max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint Clamp(uint value, uint min, uint max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Clamp(ulong value, ulong min, ulong max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Floor(decimal d)
        {
            return decimal.Floor(d);
        }

        public static double IEEERemainder(double x, double y)
        {
            if (double.IsNaN(x))
            {
                return x; // IEEE 754-2008: NaN payload must be preserved
            }

            if (double.IsNaN(y))
            {
                return y; // IEEE 754-2008: NaN payload must be preserved
            }

            var regularMod = x % y;

            if (double.IsNaN(regularMod))
            {
                return double.NaN;
            }

            if ((regularMod == 0) && double.IsNegative(x))
            {
                return double.NegativeZero;
            }

            var alternativeResult = (regularMod - (Abs(y) * Sign(x)));

            if (Abs(alternativeResult) == Abs(regularMod))
            {
                var divisionResult = x / y;
                var roundedResult = Round(divisionResult);

                if (Abs(roundedResult) > Abs(divisionResult))
                {
                    return alternativeResult;
                }
                else
                {
                    return regularMod;
                }
            }

            if (Abs(alternativeResult) < Abs(regularMod))
            {
                return alternativeResult;
            }
            else
            {
                return regularMod;
            }
        }

        public static double Log(double a, double newBase)
        {
            if (double.IsNaN(a))
            {
                return a; // IEEE 754-2008: NaN payload must be preserved
            }

            if (double.IsNaN(newBase))
            {
                return newBase; // IEEE 754-2008: NaN payload must be preserved
            }

            if (newBase == 1)
            {
                return double.NaN;
            }

            if ((a != 1) && ((newBase == 0) || double.IsPositiveInfinity(newBase)))
            {
                return double.NaN;
            }

            return (Log(a) / Log(newBase));
        }

        [NonVersionable]
        public static byte Max(byte val1, byte val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Max(decimal val1, decimal val2)
        {
            return decimal.Max(val1, val2);
        }

        public static double Max(double val1, double val2)
        {
            // This matches the IEEE 754:2019 `maximum` function
            //
            // It propagates NaN inputs back to the caller and
            // otherwise returns the larger of the inputs. It
            // treats +0 as larger than -0 as per the specification.

            if ((val1 > val2) || double.IsNaN(val1))
            {
                return val1;
            }

            if (val1 == val2)
            {
                return double.IsNegative(val1) ? val2 : val1;
            }

            return val2;
        }

        [NonVersionable]
        public static short Max(short val1, short val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        [NonVersionable]
        public static int Max(int val1, int val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        [NonVersionable]
        public static long Max(long val1, long val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        [CLSCompliant(false)]
        [NonVersionable]
        public static sbyte Max(sbyte val1, sbyte val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }
        
        public static float Max(float val1, float val2)
        {
            // This matches the IEEE 754:2019 `maximum` function
            //
            // It propagates NaN inputs back to the caller and
            // otherwise returns the larger of the inputs. It
            // treats +0 as larger than -0 as per the specification.

            if ((val1 > val2) || float.IsNaN(val1))
            {
                return val1;
            }

            if (val1 == val2)
            {
                return float.IsNegative(val1) ? val2 : val1;
            }

            return val2;
        }

        [CLSCompliant(false)]
        [NonVersionable]
        public static ushort Max(ushort val1, ushort val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        [CLSCompliant(false)]
        [NonVersionable]
        public static uint Max(uint val1, uint val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        [CLSCompliant(false)]
        [NonVersionable]
        public static ulong Max(ulong val1, ulong val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        public static double MaxMagnitude(double x, double y)
        {
            // This matches the IEEE 754:2019 `maximumMagnitude` function
            //
            // It propagates NaN inputs back to the caller and
            // otherwise returns the input with a larger magnitude.
            // It treats +0 as larger than -0 as per the specification.

            double ax = Abs(x);
            double ay = Abs(y);

            if ((ax > ay) || double.IsNaN(ax))
            {
                return x;
            }

            if (ax == ay)
            {
                return double.IsNegative(x) ? y : x;
            }

            return y;
        }

        [NonVersionable]
        public static byte Min(byte val1, byte val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Min(decimal val1, decimal val2)
        {
            return decimal.Min(val1, val2);
        }

        public static double Min(double val1, double val2)
        {
            // This matches the IEEE 754:2019 `minimum` function
            //
            // It propagates NaN inputs back to the caller and
            // otherwise returns the larger of the inputs. It
            // treats +0 as larger than -0 as per the specification.

            if ((val1 < val2) || double.IsNaN(val1))
            {
                return val1;
            }

            if (val1 == val2)
            {
                return double.IsNegative(val1) ? val1 : val2;
            }

            return val2;
        }

        [NonVersionable]
        public static short Min(short val1, short val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        [NonVersionable]
        public static int Min(int val1, int val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        [NonVersionable]
        public static long Min(long val1, long val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        [CLSCompliant(false)]
        [NonVersionable]
        public static sbyte Min(sbyte val1, sbyte val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        public static float Min(float val1, float val2)
        {
            // This matches the IEEE 754:2019 `minimum` function
            //
            // It propagates NaN inputs back to the caller and
            // otherwise returns the larger of the inputs. It
            // treats +0 as larger than -0 as per the specification.

            if ((val1 < val2) || float.IsNaN(val1))
            {
                return val1;
            }

            if (val1 == val2)
            {
                return float.IsNegative(val1) ? val1 : val2;
            }

            return val2;
        }

        [CLSCompliant(false)]
        [NonVersionable]
        public static ushort Min(ushort val1, ushort val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        [CLSCompliant(false)]
        [NonVersionable]
        public static uint Min(uint val1, uint val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        [CLSCompliant(false)]
        [NonVersionable]
        public static ulong Min(ulong val1, ulong val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        public static double MinMagnitude(double x, double y)
        {
            // This matches the IEEE 754:2019 `minimumMagnitude` function
            //
            // It propagates NaN inputs back to the caller and
            // otherwise returns the input with a larger magnitude.
            // It treats +0 as larger than -0 as per the specification.

            double ax = Abs(x);
            double ay = Abs(y);

            if ((ax < ay) || double.IsNaN(ax))
            {
                return x;
            }

            if (ax == ay)
            {
                return double.IsNegative(x) ? x : y;
            }

            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Round(decimal d)
        {
            return decimal.Round(d, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Round(decimal d, int decimals)
        {
            return decimal.Round(d, decimals);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Round(decimal d, MidpointRounding mode)
        {
            return decimal.Round(d, 0, mode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Round(decimal d, int decimals, MidpointRounding mode)
        {
            return decimal.Round(d, decimals, mode);
        }

        [Intrinsic]
        public static double Round(double a)
        {
            // ************************************************************************************
            // IMPORTANT: Do not change this implementation without also updating MathF.Round(float),
            //            FloatingPointUtils::round(double), and FloatingPointUtils::round(float)
            // ************************************************************************************

            // This is based on the 'Berkeley SoftFloat Release 3e' algorithm

            ulong bits = (ulong)BitConverter.DoubleToInt64Bits(a);
            int exponent = double.ExtractExponentFromBits(bits);

            if (exponent <= 0x03FE)
            {
                if ((bits << 1) == 0)
                {
                    // Exactly +/- zero should return the original value
                    return a;
                }

                // Any value less than or equal to 0.5 will always round to exactly zero
                // and any value greater than 0.5 will always round to exactly one. However,
                // we need to preserve the original sign for IEEE compliance.

                double result = ((exponent == 0x03FE) && (double.ExtractSignificandFromBits(bits) != 0)) ? 1.0 : 0.0;
                return CopySign(result, a);
            }

            if (exponent >= 0x0433)
            {
                // Any value greater than or equal to 2^52 cannot have a fractional part,
                // So it will always round to exactly itself.

                return a;
            }

            // The absolute value should be greater than or equal to 1.0 and less than 2^52
            Debug.Assert((0x03FF <= exponent) && (exponent <= 0x0432));

            // Determine the last bit that represents the integral portion of the value
            // and the bits representing the fractional portion

            ulong lastBitMask = 1UL << (0x0433 - exponent);
            ulong roundBitsMask = lastBitMask - 1;

            // Increment the first fractional bit, which represents the midpoint between
            // two integral values in the current window.

            bits += lastBitMask >> 1;

            if ((bits & roundBitsMask) == 0)
            {
                // If that overflowed and the rest of the fractional bits are zero
                // then we were exactly x.5 and we want to round to the even result

                bits &= ~lastBitMask;
            }
            else
            {
                // Otherwise, we just want to strip the fractional bits off, truncating
                // to the current integer value.

                bits &= ~roundBitsMask;
            }

            return BitConverter.Int64BitsToDouble((long)bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Round(double value, int digits)
        {
            return Round(value, digits, MidpointRounding.ToEven);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Round(double value, MidpointRounding mode)
        {
            return Round(value, 0, mode);
        }

        public static unsafe double Round(double value, int digits, MidpointRounding mode)
        {
            if ((digits < 0) || (digits > maxRoundingDigits))
            {
                throw new ArgumentOutOfRangeException(nameof(digits), SR.ArgumentOutOfRange_RoundingDigits);
            }

            if (mode < MidpointRounding.ToEven || mode > MidpointRounding.ToPositiveInfinity)
            {
                throw new ArgumentException(SR.Format(SR.Argument_InvalidEnumValue, mode, nameof(MidpointRounding)), nameof(mode));
            }

            if (Abs(value) < doubleRoundLimit)
            {
                var power10 = roundPower10Double[digits];

                value *= power10;

                switch (mode)
                {
                    // Rounds to the nearest value; if the number falls midway,
                    // it is rounded to the nearest value with an even least significant digit
                    case MidpointRounding.ToEven:
                    {
                        value = Round(value);
                        break;
                    }
                    // Rounds to the nearest value; if the number falls midway,
                    // it is rounded to the nearest value above (for positive numbers) or below (for negative numbers)
                    case MidpointRounding.AwayFromZero:
                    {
                        double fraction = ModF(value, &value);

                        if (Abs(fraction) >= 0.5)
                        {
                            value += Sign(fraction);
                        }

                        break;
                    }
                    // Directed rounding: Round to the nearest value, toward to zero
                    case MidpointRounding.ToZero:
                    {
                        value = Truncate(value);
                        break;
                    }
                    // Directed Rounding: Round down to the next value, toward negative infinity
                    case MidpointRounding.ToNegativeInfinity:
                    {
                        value = Floor(value);
                        break;
                    }
                    // Directed rounding: Round up to the next value, toward positive infinity
                    case MidpointRounding.ToPositiveInfinity:
                    {  
                        value = Ceiling(value);
                        break;
                    }
                    default:
                    {
                        throw new ArgumentException(SR.Format(SR.Argument_InvalidEnumValue, mode, nameof(MidpointRounding)), nameof(mode));
                    }
                }
                
                value /= power10;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(decimal value)
        {
            return decimal.Sign(value);
        }

        public static int Sign(double value)
        {
            if (value < 0)
            {
                return -1;
            }
            else if (value > 0)
            {
                return 1;
            }
            else if (value == 0)
            {
                return 0;
            }

            throw new ArithmeticException(SR.Arithmetic_NaN);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(short value)
        {
            return Sign((int)value);
        }

        public static int Sign(int value)
        {
            return unchecked(value >> 31 | (int)((uint)-value >> 31));
        }

        public static int Sign(long value)
        {
            return unchecked((int)(value >> 63 | (long)((ulong)-value >> 63)));
        }

        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(sbyte value)
        {
            return Sign((int)value);
        }

        public static int Sign(float value)
        {
            if (value < 0)
            {
                return -1;
            }
            else if (value > 0)
            {
                return 1;
            }
            else if (value == 0)
            {
                return 0;
            }

            throw new ArithmeticException(SR.Arithmetic_NaN);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Truncate(decimal d)
        {
            return decimal.Truncate(d);
        }

        public static unsafe double Truncate(double d)
        {
            ModF(d, &d);
            return d;
        }

        [DoesNotReturn]
        private static void ThrowMinMaxException<T>(T min, T max)
        {
            throw new ArgumentException(SR.Format(SR.Argument_MinMaxValue, min, max));
        }
    }
}
