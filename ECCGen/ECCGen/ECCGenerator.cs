namespace ECCGen {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using x360NANDManager;

    public sealed class ECCGenerator : Utils {
        #region ECCType enum

        public enum ECCType {
            Auto,
            RGH1,
            RGH2,
            RGX
        }

        #endregion

        #region MotherBoard enum

        public enum MotherBoard {
            Auto,
            Corona,
            Falcon,
            Jasper,
            Trinity,
            Xenon,
            Zephyr
        }

        #endregion

        #region SpareType enum

        public enum SpareType {
            None,
            SmallBlock,
            SmallBlock2,
            BigBlock,
            Auto
        }

        #endregion

        private const string BaseName = "ECCGen v{0}.{1} (Build: {2}) {3}";

        private static readonly Dictionary<int, List<PatchObject>> Patches = new Dictionary<int, List<PatchObject>>();

        internal static readonly Version Ver = Assembly.GetExecutingAssembly().GetName().Version;

        public ECCGenerator(string patchFile) {
            if(SetPatches(patchFile))
                return;
            UpdateStatus("Patch file parsing failed!\r\nUsing built in patches instead...");
            MakeDefaultPatches();
        }

        public ECCGenerator() {
            MakeDefaultPatches();
        }

        public static string Version {
            get {
#if DEBUG
                return string.Format(BaseName, Ver.Major, Ver.Minor, Ver.Build, "DEBUG BUILD");
#elif ALPHA
                return string.Format(BaseName, Ver.Major, Ver.Minor, Ver.Build, Ver.Revision > 0 ? string.Format("ALPHA{0}", Ver.Revision) : "ALPHA");
#elif BETA
                return string.Format(BaseName, Ver.Major, Ver.Minor, Ver.Build, Ver.Revision > 0 ? string.Format("BETA{0}", Ver.Revision) : "BETA");
#else
                return string.Format(BaseName, Ver.Major, Ver.Minor, Ver.Build, "");
#endif
            }
        }

        private static void MakeDefaultPatches() {
            Patches.Clear();
            Patches.Add(4577, new List<PatchObject>(new[] {
                                                          new PatchObject(0x53C0, 0x7BEB0620, 0x48000168),
                                                          new PatchObject(0x55B8, 0x409A0010, 0x60000000),
                                                          new PatchObject(0x5D54, 0x480018C5, 0x60000000),
                                                          new PatchObject(0x5DB0, 0x480000A1, 0x60000000)
                                                          }));
            Patches.Add(5772, new List<PatchObject>(new[] {
                                                          new PatchObject(0x4F0, 0x48006239, 0x60000000),
                                                          new PatchObject(0x6860, 0x7BEB0620, 0x48000168),
                                                          new PatchObject(0x6A58, 0x409A0010, 0x60000000),
                                                          new PatchObject(0x7168, 0x480018E1, 0x60000000),
                                                          new PatchObject(0x71b8, 0x419A0014, 0x48000014)
                                                          }));
            Patches.Add(6752, new List<PatchObject>(new[] {
                                                          new PatchObject(0x68A8, 0x7BEB0620, 0x48000168),
                                                          new PatchObject(0x6AA0, 0x409A0010, 0x60000000),
                                                          new PatchObject(0x71B0, 0x480018D9, 0x60000000),
                                                          new PatchObject(0x7200, 0x419A0014, 0x48000014)
                                                          }));
            Patches.Add(9188, new List<PatchObject>(new[] {
                                                          new PatchObject(0x4d10, 0x7BEB0620, 0x48000168),
                                                          new PatchObject(0x4f08, 0x409a0010, 0x60000000),
                                                          new PatchObject(0x5618, 0x480018e1, 0x60000000),
                                                          new PatchObject(0x5678, 0x480000b9, 0x60000000)
                                                          }));
            Patches.Add(13121, new List<PatchObject>(new[] {
                                                           new PatchObject(0x5048, 0x7BEB0620, 0x48000168),
                                                           new PatchObject(0x58ac, 0x480001C5, 0x60000000),
                                                           new PatchObject(0x5958, 0x480018E1, 0x60000000),
                                                           new PatchObject(0x59B8, 0x480000B9, 0x60000000)
                                                           }));
        }

        public static bool SetPatches(string patchFile) {
            Patches.Clear();
            throw new NotImplementedException();
        }

        public static event EventHandler<EventArg<string>> Status;

        internal static void UpdateStatus(string msg) {
            var handler = Status;
            if(handler != null)
                handler(null, new EventArg<string>(msg));
        }

        public byte[] GenerateECC(ECCArgs args) {
            args.CheckValidSettings();
            var builder = new Builder();
            throw new NotImplementedException();
        }

        #region Nested type: ECCArgs

        public struct ECCArgs {
            internal ECCType HackType;
            internal SpareType SpareType;
            // ReSharper disable UnassignedField.Global
            // ReSharper disable MemberCanBeInternal
            public byte[] SourceNANDData;
            public string SourceNANDPath;
            public byte[] TargetSMCData;
            public string TargetSMCPath;
            public byte[] TargetKVData;
            public string TargetKVPath;
            public byte[] TargetCBAData;
            public string TargetCBAPath;
            public byte[] TargetCBBData;
            public string TargetCBBPath;
            public byte[] TargetCDData;
            public string TargetCDPath;
            public byte[] TargetXeLLData;
            public string TargetXeLLPath;
            // ReSharper restore MemberCanBeInternal
            // ReSharper restore UnassignedField.Global

            public ECCArgs(ECCType hackType = ECCType.Auto, SpareType spareType = SpareType.Auto) : this() {
                SpareType = spareType;
                HackType = hackType;
            }

            internal void CheckValidSettings() {
                UpdateStatus("Checking Settings...");
                var hasSourceNAND = (SourceNANDData != null && !string.IsNullOrEmpty(SourceNANDPath));
                if(!hasSourceNAND) {
                    if(SpareType == SpareType.Auto || HackType == ECCType.RGH2)
                        throw new InvalidOperationException("Your settings are incomplete or unsupported");
                }
                if(SourceNANDData == null)
                    NANDParser.Source = SourceNANDPath;
                if(SpareType == SpareType.Auto) {
                    UpdateStatus("Checking Spare Type...");
                    SpareType = SourceNANDData != null ? NANDParser.GetSpareType(ref SourceNANDData) : NANDParser.GetSpareType(SourceNANDPath);
                }

                if(TargetSMCData == null || string.IsNullOrEmpty(TargetSMCPath) && !hasSourceNAND)
                    throw new InvalidOperationException("Your settings are incomplete or unsupported (You must etheir supply a source nand or SMCDATA/Path to SMC File");
                if(TargetSMCData == null) {
                    UpdateStatus("Grabbing SMC Data...");
                    if(!string.IsNullOrEmpty(TargetSMCPath))
                        TargetCBAData = File.ReadAllBytes(TargetSMCPath);
                    else if (SourceNANDData == null)
                        TargetSMCData = NANDParser.GrabData(NANDParser.NANDParts.SMC);
                    else
                        TargetSMCData = NANDParser.GrabData(ref SourceNANDData, NANDParser.NANDParts.SMC);
                }

                #region CB_A and CB_B

                if(!hasSourceNAND && TargetCBAData == null && string.IsNullOrEmpty(TargetCBAPath) && HackType != ECCType.RGX) {
                    if(HackType == ECCType.RGH1)
                        throw new InvalidOperationException("Your settings are incomplete or unsupported (You must etheir supply a source nand or CB DATA/Path to CB File");
                    throw new InvalidOperationException("Your settings are incomplete or unsupported (You must etheir supply a source nand or CB_A DATA/Path to CB_A File");
                }
                UpdateStatus(HackType == ECCType.RGH1 ? "Grabbing CB Data..." : "Grabbing CB_A Data...");
                if(TargetCBAData == null && !string.IsNullOrEmpty(TargetCBAPath) && HackType != ECCType.RGX)
                    TargetCBAData = File.ReadAllBytes(TargetCBAPath);
                else if(HackType == ECCType.RGX) {
                    UpdateStatus("Using Built in MFG CB_A");
                    TargetCBAData = GetBuiltInData("CBMFG");
                }
                else if (SourceNANDData == null)
                    TargetCBAData = NANDParser.GrabData(NANDParser.NANDParts.CBA);
                else
                    TargetCBAData = NANDParser.GrabData(ref SourceNANDData, NANDParser.NANDParts.CBA);
                if(HackType != ECCType.RGH1) {
                    if(TargetCBBData == null && string.IsNullOrEmpty(TargetCBBPath))
                        throw new InvalidOperationException("Your settings are incomplete or unsupported (You must etheir supply a source nand or CB_B DATA/Path to CB_B File");
                    if (TargetCBBData == null) {
                        UpdateStatus("Grabbing CB_B Data...");
                        if(TargetCBBData == null && !string.IsNullOrEmpty(TargetCBBPath))
                            TargetCBBData = File.ReadAllBytes(TargetCBBPath);
                        else if (SourceNANDData == null)
                            TargetCBBData = NANDParser.GrabData(NANDParser.NANDParts.CBB);
                        else
                            TargetCBBData = NANDParser.GrabData(ref SourceNANDData, NANDParser.NANDParts.CBB);
                    }
                }

                #endregion CB_A and CB_B

                #region CD

                UpdateStatus("Grabbing CD Data...");
                if(TargetCDData == null && !string.IsNullOrEmpty(TargetCDPath))
                    TargetCDData = File.ReadAllBytes(TargetCDPath);
                if(TargetCDData == null && string.IsNullOrEmpty(TargetCDPath)) {
                    UpdateStatus("Using Built in CD");
                    TargetCDData = GetBuiltInData("CD");
                }
                #endregion CD

                #region XeLL

                UpdateStatus("Grabbing XeLL Data...");
                if(TargetXeLLData == null && !string.IsNullOrEmpty(TargetXeLLPath))
                    TargetXeLLData = File.ReadAllBytes(TargetXeLLPath);
                else if(TargetXeLLData == null && string.IsNullOrEmpty(TargetXeLLPath)) {
                    UpdateStatus("Using Built in XeLL");
                    TargetXeLLData = GetBuiltInData("XELL");
                }

                #endregion XeLL
            }
        }

        #endregion XeLL

        #region Nested type: PatchObject

        internal struct PatchObject {
            private readonly int _offset;
            private readonly byte[] _patched;
            private readonly byte[] _plain;

            public PatchObject(int offset, uint plain, uint patched) {
                _offset = offset;
                _plain = BitConverter.GetBytes(Swap32(plain));
                _patched = BitConverter.GetBytes(Swap32(patched));
            }

            internal void ApplyCBPatch(ref byte[] data, int patchCount, int patchnum = 1) {
                UpdateStatus(string.Format("Applying Patch #{0} of {1} @ Offset: 0x{2:X}", patchnum, patchCount, _offset));
                if(data.Length < _offset + 4)
                    throw new ArgumentOutOfRangeException();
                for(var i = 0; i < 4; i++)
                    data[_offset + i] = Convert.ToByte((_plain[i] ^ data[_offset + i]) ^ _patched[i]);
            }
        }

        #endregion
    }
}
