using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DPWorldEncryption
{
    public sealed class SecurityConfig
    {
        private static System.Text.Encoding _Encoding = System.Text.Encoding.UTF8;

        public static System.Text.Encoding Encoding
        {
            get
            {
                return _Encoding;
            }
            set
            {
                _Encoding = value;
            }
        }

    }
}