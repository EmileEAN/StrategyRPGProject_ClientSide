using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EEANWorks
{
    public static class MTRandom
    {
        /* Period parameters */
        private const int MT_N = 624;
        private const int MT_M = 397;
        private const ulong MATRIX_A = 0x9908b0dfUL;   /* constant vector a */
        private const ulong UPPER_MASK = 0x80000000UL; /* most significant w-r bits */
        private const ulong LOWER_MASK = 0x7fffffffUL; /* least significant r bits */

        private static ulong[] m_mt = new ulong[MT_N]; /* the array for the state vector  */
        private static int m_mti = MT_N + 1; /* mti==MT_N+1 means mt[MT_N] is not initialized */


        public static void RandInit()
        {
            ulong s = (ulong)DateTime.Now.Ticks;

            m_mt[0] = s & 0xffffffffUL;
            for (m_mti = 1; m_mti < MT_N; m_mti++)
            {
                m_mt[m_mti] =
                    (1812433253UL * (m_mt[m_mti - 1] ^ (m_mt[m_mti - 1] >> 30)) + (ulong)m_mti);

                m_mt[m_mti] &= 0xffffffffUL;
                /* for >32 bit machines */
            }
        }

        public static int GetRandInt(int _a, int _b)
        {
            ulong y;
            ulong[] mag01 = new ulong[2] { 0x0UL, MATRIX_A };
            /* mag01[x] = x * MATRIX_A  for x=0,1 */

            if (m_mti >= MT_N)
            { /* generate N words at one time */
                int kk;

                if (m_mti == MT_N + 1)   /* if randInit() has not been called, */
                    RandInit();

                for (kk = 0; kk < MT_N - MT_M; kk++)
                {
                    y = (m_mt[kk] & UPPER_MASK) | (m_mt[kk + 1] & LOWER_MASK);
                    m_mt[kk] = m_mt[kk + MT_M] ^ (y >> 1) ^ mag01[y & 0x1UL];
                }
                for (; kk < MT_N - 1; kk++)
                {
                    y = (m_mt[kk] & UPPER_MASK) | (m_mt[kk + 1] & LOWER_MASK);
                    m_mt[kk] = m_mt[kk + (MT_M - MT_N)] ^ (y >> 1) ^ mag01[y & 0x1UL];
                }
                y = (m_mt[MT_N - 1] & UPPER_MASK) | (m_mt[0] & LOWER_MASK);
                m_mt[MT_N - 1] = m_mt[MT_M - 1] ^ (y >> 1) ^ mag01[y & 0x1UL];

                m_mti = 0;
            }

            y = m_mt[m_mti++];

            /* Tempering */
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680UL;
            y ^= (y << 15) & 0xefc60000UL;
            y ^= (y >> 18);

            return Convert.ToInt32(y % Convert.ToUInt64(_b - (_a - 1))) + _a;
        }

        public static string GetRandString(int _size, bool _includeNumbers, bool _includeUpperCaseCharacters, bool _includeLowerCaseCharacters)
        {
            string result = string.Empty;

            if (!_includeNumbers && !_includeUpperCaseCharacters && !_includeLowerCaseCharacters)
                return result;

            int randMin;
            int randMax;
            int characterType;
            int randomInt;
            char c;

            for (int i = 0; i < _size; i++)
            {
                while (true) // Choose the characterType and set the range of rand numbers
                {
                    characterType = GetRandInt(1, 3);
                    if (characterType == 1 && _includeNumbers)
                    {
                        randMin = 48;
                        randMax = 57;
                        break;
                    }
                    else if (characterType == 2 && _includeUpperCaseCharacters)
                    {
                        randMin = 65;
                        randMax = 90;
                        break;
                    }
                    else if (characterType == 3 && _includeLowerCaseCharacters)
                    {
                        randMin = 97;
                        randMax = 122;
                        break;
                    }
                }

                randomInt = GetRandInt(randMin, randMax);
                c = Convert.ToChar(randomInt);

                result += c;
            }

            return result;
        }
    }
}

