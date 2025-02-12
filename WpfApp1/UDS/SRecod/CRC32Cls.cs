using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WpfApp1.UDS.SRecord
{
    public class CRC32Cls
    {
		protected static ulong[] Crc32Table = new ulong[256]
		{
			0uL, 1996959894uL, 3993919788uL, 2567524794uL, 124634137uL, 1886057615uL, 3915621685uL, 2657392035uL, 249268274uL, 2044508324uL,
			3772115230uL, 2547177864uL, 162941995uL, 2125561021uL, 3887607047uL, 2428444049uL, 498536548uL, 1789927666uL, 4089016648uL, 2227061214uL,
			450548861uL, 1843258603uL, 4107580753uL, 2211677639uL, 325883990uL, 1684777152uL, 4251122042uL, 2321926636uL, 335633487uL, 1661365465uL,
			4195302755uL, 2366115317uL, 997073096uL, 1281953886uL, 3579855332uL, 2724688242uL, 1006888145uL, 1258607687uL, 3524101629uL, 2768942443uL,
			901097722uL, 1119000684uL, 3686517206uL, 2898065728uL, 853044451uL, 1172266101uL, 3705015759uL, 2882616665uL, 651767980uL, 1373503546uL,
			3369554304uL, 3218104598uL, 565507253uL, 1454621731uL, 3485111705uL, 3099436303uL, 671266974uL, 1594198024uL, 3322730930uL, 2970347812uL,
			795835527uL, 1483230225uL, 3244367275uL, 3060149565uL, 1994146192uL, 31158534uL, 2563907772uL, 4023717930uL, 1907459465uL, 112637215uL,
			2680153253uL, 3904427059uL, 2013776290uL, 251722036uL, 2517215374uL, 3775830040uL, 2137656763uL, 141376813uL, 2439277719uL, 3865271297uL,
			1802195444uL, 476864866uL, 2238001368uL, 4066508878uL, 1812370925uL, 453092731uL, 2181625025uL, 4111451223uL, 1706088902uL, 314042704uL,
			2344532202uL, 4240017532uL, 1658658271uL, 366619977uL, 2362670323uL, 4224994405uL, 1303535960uL, 984961486uL, 2747007092uL, 3569037538uL,
			1256170817uL, 1037604311uL, 2765210733uL, 3554079995uL, 1131014506uL, 879679996uL, 2909243462uL, 3663771856uL, 1141124467uL, 855842277uL,
			2852801631uL, 3708648649uL, 1342533948uL, 654459306uL, 3188396048uL, 3373015174uL, 1466479909uL, 544179635uL, 3110523913uL, 3462522015uL,
			1591671054uL, 702138776uL, 2966460450uL, 3352799412uL, 1504918807uL, 783551873uL, 3082640443uL, 3233442989uL, 3988292384uL, 2596254646uL,
			62317068uL, 1957810842uL, 3939845945uL, 2647816111uL, 81470997uL, 1943803523uL, 3814918930uL, 2489596804uL, 225274430uL, 2053790376uL,
			3826175755uL, 2466906013uL, 167816743uL, 2097651377uL, 4027552580uL, 2265490386uL, 503444072uL, 1762050814uL, 4150417245uL, 2154129355uL,
			426522225uL, 1852507879uL, 4275313526uL, 2312317920uL, 282753626uL, 1742555852uL, 4189708143uL, 2394877945uL, 397917763uL, 1622183637uL,
			3604390888uL, 2714866558uL, 953729732uL, 1340076626uL, 3518719985uL, 2797360999uL, 1068828381uL, 1219638859uL, 3624741850uL, 2936675148uL,
			906185462uL, 1090812512uL, 3747672003uL, 2825379669uL, 829329135uL, 1181335161uL, 3412177804uL, 3160834842uL, 628085408uL, 1382605366uL,
			3423369109uL, 3138078467uL, 570562233uL, 1426400815uL, 3317316542uL, 2998733608uL, 733239954uL, 1555261956uL, 3268935591uL, 3050360625uL,
			752459403uL, 1541320221uL, 2607071920uL, 3965973030uL, 1969922972uL, 40735498uL, 2617837225uL, 3943577151uL, 1913087877uL, 83908371uL,
			2512341634uL, 3803740692uL, 2075208622uL, 213261112uL, 2463272603uL, 3855990285uL, 2094854071uL, 198958881uL, 2262029012uL, 4057260610uL,
			1759359992uL, 534414190uL, 2176718541uL, 4139329115uL, 1873836001uL, 414664567uL, 2282248934uL, 4279200368uL, 1711684554uL, 285281116uL,
			2405801727uL, 4167216745uL, 1634467795uL, 376229701uL, 2685067896uL, 3608007406uL, 1308918612uL, 956543938uL, 2808555105uL, 3495958263uL,
			1231636301uL, 1047427035uL, 2932959818uL, 3654703836uL, 1088359270uL, 936918000uL, 2847714899uL, 3736837829uL, 1202900863uL, 817233897uL,
			3183342108uL, 3401237130uL, 1404277552uL, 615818150uL, 3134207493uL, 3453421203uL, 1423857449uL, 601450431uL, 3009837614uL, 3294710456uL,
			1567103746uL, 711928724uL, 3020668471uL, 3272380065uL, 1510334235uL, 755167117uL
		};

		public static ulong GetCRC32Str(string path)
		{
			Stream stream = new FileStream(path, FileMode.Open);
			BinaryReader binaryReader = new BinaryReader(stream);
			ulong num = 4294967295uL;
			while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
			{
				byte b = binaryReader.ReadByte();
				num = (num >> 8) ^ Crc32Table[(num ^ b) & 0xFF];
			}
			stream.Close();
			binaryReader.Close();
			return num ^ 0xFFFFFFFFu;
		}

        public static ulong GetCRC32Str(byte[] bytes)
        {
            ulong crc32 = 0xFFFFFFFF;
            foreach (var b in bytes)
            {
                crc32 = (crc32 >> 8) ^ Crc32Table[(crc32 ^ b) & 0xff];
            }
            crc32 ^= 0xffffffff;
            return crc32;
        }

		private static uint[] GenerateCRC32Table()
		{
			uint[] table = new uint[256];

			for (uint i = 0; i < 256; i++)
			{
				uint crc32 = i;

				for (int j = 0; j < 8; j++)
				{
					if ((crc32 & 1) == 1)
					{
						crc32 = (crc32 >> 1) ^ 0xEDB88320;
					}
					else
					{
						crc32 >>= 1;
					}
				}

				table[i] = crc32;
			}

			return table;
		}

		public static uint GetCRC32(byte[] bytes)
		{
			uint[] crc32Table = GenerateCRC32Table();
			uint crc32 = 0xFFFFFFFF;
			foreach (var b in bytes)
			{
				crc32 = (crc32 >> 8) ^ crc32Table[(crc32 ^ b) & 0xff];
			}
			crc32 ^= 0xffffffff;
			return crc32;
		}

		public static ulong GetSrecCRC32Str(string path)
		{
			Stream stream = new FileStream(path, FileMode.Open);
			BinaryReader binaryReader = new BinaryReader(stream);
			FileStream stream2 = new FileStream(path, FileMode.Open, FileAccess.Read);
			StreamReader streamReader = new StreamReader(stream2, Encoding.Default);
			string empty = string.Empty;
			uint num = 0u;
			int num2 = -1;
			ulong num3 = 4294967295uL;
			while ((empty = streamReader.ReadLine()) != null)
			{
				string text = empty.Trim();
				if (text.Substring(0, 2).CompareTo("S2") != 0)
				{
					continue;
				}
				ushort num4 = Convert.ToUInt16(text.Substring(2, 2), 16);
				uint num5 = Convert.ToUInt32(text.Substring(4, 8), 16);
				if (num2 < 0)
				{
					num2 = (int)num5;
				}
				if (num != 0)
				{
					if (num5 >= num)
					{
						for (uint num6 = 0u; num6 < num5 - num; num6++)
						{
							byte b = byte.MaxValue;
							num3 = (num3 >> 8) ^ Crc32Table[(num3 ^ b) & 0xFF];
						}
					}
					else
					{
						Console.WriteLine("addr error;");
						Console.ReadKey();
					}
				}
				num = num5 + num4 - 4;
				for (int i = 0; i < num4 - 3 - 1; i++)
				{
					byte b = Convert.ToByte(text.Substring(i * 2 + 10, 2), 16);
					num3 = (num3 >> 8) ^ Crc32Table[(num3 ^ b) & 0xFF];
				}
			}
			stream.Close();
			binaryReader.Close();
			return num3 ^ 0xFFFFFFFFu;
		}
	}

	public class CMACCalculator
	{
		public static byte[] AESEncrypt(byte[] key, byte[] iv, byte[] data)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.None;
				using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
				{
					cs.Write(data, 0, data.Length);
					cs.FlushFinalBlock();
					return ms.ToArray();
				}
			}
		}

		static byte[] Rol(byte[] b)
		{
			byte[] r = new byte[b.Length];
			byte carry = 0;
			for (int i = b.Length - 1; i >= 0; i--)
			{
				ushort u = (ushort)(b[i] << 1);
				r[i] = (byte)((u & 0xff) + carry);
				carry = (byte)((u & 0xff00) >> 8);
			}
			return r;
		}

		public static byte[] AESCMAC(byte[] key, byte[] data)
		{
			// SubKey generation
			// step 1, AES-128 with key K is applied to an all-zero input block.
			byte[] L = AESEncrypt(key, new byte[16], new byte[16]);
			// step 2, K1 is derived through the following operation:
			byte[] FirstSubkey = Rol(L); //If the most significant bit of L is equal to 0, K1 is the left-shift of L by 1 bit.
			if ((L[0] & 0x80) == 0x80)
				FirstSubkey[15] ^= 0x87; // Otherwise, K1 is the exclusive-OR of const_Rb and the left-shift of L by 1 bit.
										 // step 3, K2 is derived through the following operation:
			byte[] SecondSubkey = Rol(FirstSubkey); // If the most significant bit of K1 is equal to 0, K2 is the left-shift of K1 by 1 bit.
			if ((FirstSubkey[0] & 0x80) == 0x80)
				SecondSubkey[15] ^= 0x87; // Otherwise, K2 is the exclusive-OR of const_Rb and the left-shift of K1 by 1 bit.
										  // MAC computing
			if (((data.Length != 0) && (data.Length % 16 == 0)) == true)
			{
				// If the size of the input message block is equal to a positive multiple of the block size (namely, 128 bits),
				// the last block shall be exclusive-OR'ed with K1 before processing
				for (int j = 0; j < FirstSubkey.Length; j++)
					data[data.Length - 16 + j] ^= FirstSubkey[j];
			}
			else
			{
				// Otherwise, the last block shall be padded with 10^i
				byte[] padding = new byte[16 - data.Length % 16];
				padding[0] = 0x80;
				
				data = data.Concat<byte>(padding.AsEnumerable()).ToArray();
				// and exclusive-OR'ed with K2
				for (int j = 0; j < SecondSubkey.Length; j++)
					data[data.Length - 16 + j] ^= SecondSubkey[j];
			}
			// The result of the previous process will be the input of the last encryption.
			byte[] encResult = AESEncrypt(key, new byte[16], data);
			byte[] HashValue = new byte[16];
			Array.Copy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);
			return HashValue;
		}

        public static byte[] Test()
        {
            byte[] key = new byte[] { 
				0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 
				0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c };
            byte[] inputMsg =
            {
                 0x6b, 0xc1, 0xbe, 0xe2, 0x2e, 0x40, 0x9f, 0x96,
                 0xe9, 0x3d, 0x7e, 0x11, 0x73, 0x93, 0x17, 0x2a,
                 0xae, 0x2d, 0x8a, 0x57, 0x1e, 0x03, 0xac, 0x9c,
                 0x9e, 0xb7, 0x6f, 0xac, 0x45, 0xaf, 0x8e, 0x51,
                 0x30, 0xc8, 0x1c, 0x46, 0xa3, 0x5c, 0xe4, 0x11,
                 0xe5, 0xfb, 0xc1, 0x19, 0x1a, 0x0a, 0x52, 0xef,
                 0xf6, 0x9f, 0x24, 0x45, 0xdf, 0x4f, 0x9b, 0x17,
                 0xad, 0x2b, 0x41, 0x7b, 0xe6, 0x6c, 0x37, 0x10
			};

			//         byte[] cmac = AESCMAC(key, inputMsg);
			//Console.WriteLine("Cmac:   {0}", string.Join("-", cmac));
			byte[] cmac =AES128Cmac.AES_CMAC(key, inputMsg, inputMsg.Length);
			Console.WriteLine("Cmac:   {0}", string.Join("-", cmac));

            return cmac;

        }
    }

	public class AES128Cmac
    {
        static readonly byte[] const_Rb = {
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x87};

		//void xor_128(unsigned char* a, unsigned char* b, unsigned char* out)
		//{
		//	int i;
		//	for (i = 0; i < 16; i++)
		//	{
		//      out[i] = a[i] ^ b[i];
		//	}
		//}

		static byte[] xor_128(byte[] a, byte[] b, int aindex = 0, int bIndex = 0)
        {
            byte[] output = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                output[i] = (byte)(a[i + aindex] ^ b[i + bIndex]);
            }
            return output;
        }

		/*
		 * void leftshift_onebit(unsigned char* input, unsigned char* output)
			{
			    int         i;
			    unsigned char overflow = 0;
			
			    for (i = 15; i >= 0; i--) {
			        output[i] = input[i] << 1;
			        output[i] |= overflow;
			        overflow = (input[i] & 0x80) ? 1 : 0;
			    }
			    return;
			}
		 */

		static byte[] leftshift_onebit(byte[] input)
        {
			byte[] output = new byte[input.Length];
			byte overflow = 0;
            for (int i = 15; i >= 0; i--)
            {
				output[i] = (byte)(input[i] << 1);
				output[i] |= overflow;
				overflow = (byte)((input[i] & 0x80) != 0 ? 1 : 0);
				//overflow = (input[i] & 0x80) ? 1 : 0;
			}
			return output;
		}

		/*
		 void generate_subkey(unsigned char* key, unsigned char* K1, unsigned
		*    char* K2)
		*{
		*    unsigned char L[16];
		*    unsigned char Z[16];
		*    unsigned char tmp[16];
		*    int i;
		*
		*    for (i = 0; i < 16; i++) Z[i] = 0;
		*
		*    AES_128(key, Z, L);
		*
		*    if ((L[0] & 0x80) == 0) { /* If MSB(L) = 0, then K1 = L << 1 *//*
		*		leftshift_onebit(L, K1);
		*	}
		*    else {    /* Else K1 = ( L << 1 ) (+) Rb *//*
		*        leftshift_onebit(L, tmp);
		*		xor_128(tmp, const_Rb, K1);
		*	}
		*	
		*	if ((K1[0] & 0x80) == 0)
		*	{
		*		leftshift_onebit(K1, K2);
		*	}
		*	else
		*	{
		*		leftshift_onebit(K1, tmp);
		*		xor_128(tmp, const_Rb, K2);
		*	}
		*	return;
		*}
		 */
		static void Generate_subkey(byte[] key, out byte[] K1, out byte[] K2)
        {
            byte[] L = new byte[16];
            byte[] Z = new byte[16];
            byte[] tmp = new byte[16];

            L = CMACCalculator.AESEncrypt(key, new byte[16], Z);

            if ((L[0] & 0x80) == 0)
            {
                K1 = leftshift_onebit(L);
            }
            else
            {
                tmp = leftshift_onebit(L);
                K1 = xor_128(tmp, const_Rb);
            }

            if ((K1[0] & 0x80) == 0)
            {
                K2 = leftshift_onebit(K1);
            }
            else
            {
                tmp = leftshift_onebit(K1);
                K2 = xor_128(tmp, const_Rb);
            }
        }
		/*
		void padding(unsigned char* lastb, unsigned char* pad, int length)
		{
			int j;

			/* original last block *//*
			for (j = 0; j < 16; j++)
			{
				if (j < length)
				{
					pad[j] = lastb[j];
				}
				else if (j == length)
				{
					pad[j] = 0x80;
				}
				else
				{
					pad[j] = 0x00;
				}
			}
		}
		*/
		static void padding(byte[] lastb, out byte[] pad, int length,int index = 0)
        {
            pad = new byte[16];

            for (int j = 0; j < 16; j++)
            {
                if (j < length)
                {
                    pad[j] = lastb[index + j];
                }
                else if (j == length)
                {
                    pad[j] = 0x80;
                }
                else
                {
                    pad[j] = 0x00;
                }
            }
        }

        //void AES_CMAC(unsigned char* key, unsigned char* input, int length,unsigned char* mac)
        //{
        //	unsigned char X[16], Y[16], M_last[16], padded[16];
        //	unsigned char K1[16], K2[16];
        //	int n, i, flag;
        //	generate_subkey(key, K1, K2);
        //	n = (length + 15) / 16;       /* n is number of rounds */
        //	if (n == 0)
        //	{
        //		n = 1;
        //		flag = 0;
        //	}
        //	else
        //	{
        //		if ((length % 16) == 0)
        //		{ /* last block is a complete block */
        //			flag = 1;
        //		}
        //		else
        //		{ /* last block is not complete block */
        //			flag = 0;
        //		}
        //	}
        //	if (flag)
        //	{ /* last block is complete block */
        //		xor_128(&input[16 * (n - 1)], K1, M_last);
        //	}
        //	else
        //	{
        //		padding(&input[16 * (n - 1)], padded, length % 16);
        //		xor_128(padded, K2, M_last);
        //	}
        //	for (i = 0; i < 16; i++) X[i] = 0;
        //	for (i = 0; i < n - 1; i++)
        //	{
        //		xor_128(X, &input[16 * i], Y); /* Y := Mi (+) X  */
        //		AES_128(key, Y, X);      /* X := AES-128(KEY, Y); */
        //	}
        //	xor_128(X, M_last, Y);
        //	AES_128(key, Y, X);
        //	for (i = 0; i < 16; i++)
        //	{
        //		mac[i] = X[i];
        //	}
        //}

        public static byte[] AES_CMAC(byte[] key, byte[] input,int length)
        {
			byte[] mac = new byte[16];
			byte[] X = new byte[16];
			byte[] Y = new byte[16];
			byte[] M_last = new byte[16];
			byte[] padded = new byte[16];
			int flag = 0;
			Generate_subkey(key, out byte[] K1, out byte[] K2);
			Console.WriteLine($"K1:{BitConverter.ToString(K1)}");
			Console.WriteLine($"K2:{BitConverter.ToString(K2)}");
			int n = (length + 15) / 16;
			if(n == 0)
            {
				n = 1;
            }
            else
            {
				if((length % 16) == 0)
                {
					flag = 1;
                }
            }

            if (flag == 1)
            {
                M_last = xor_128(input, K1, 16 * (n - 1));
            }
            else
            {
                padding(input, out padded, length % 16);
                M_last = xor_128(padded, K2);
            }
			//Console.WriteLine($"M_last:{BitConverter.ToString(M_last)}");
			//        for (int i = 0; i < 16; i++)
			//        {
			//X[i] = 0;
			//        }
			//X = CMACCalculator.AESEncrypt(key, new byte[16], Y);
			for (int i = 0; i < n -1; i++)
            {
                Y = xor_128(X, input, 0, 16 * i);
				//Console.WriteLine($"Y {i}:{BitConverter.ToString(Y)}");
				X = CMACCalculator.AESEncrypt(key, new byte[16], Y);
				//Console.WriteLine($"X {i}:{BitConverter.ToString(X)}");
			}
			Y = xor_128(X, M_last);
			//Console.WriteLine($"M_last Y:{BitConverter.ToString(Y)}");
			X = CMACCalculator.AESEncrypt(key, new byte[16], Y);
            for (int i = 0; i < 16; i++)
            {
				mac[i] = X[i];
            }
			return mac;
        }
    }
}
