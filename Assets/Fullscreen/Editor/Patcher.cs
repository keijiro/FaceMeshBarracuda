using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FullscreenEditor {
    // error CS0702: A constraint cannot be special class `System.Delegate'
    // Unity 2018.3.11f1 - .NET 3.5 equivalent
    // public class Patcher<T> : Patcher where T : Delegate {
    //     public Patcher(T method, T replacement) : base(method.GetMethodInfo(), replacement.GetMethodInfo()) { }

    //     public Patcher(MethodBase method, T replacement) : base(method, replacement.GetMethodInfo()) { }
    // }

    public unsafe class Patcher {

        private bool swapped;
        private MethodBase method;
        private MethodInfo replacement;
        private byte[] backup = new byte[25];

        private IntPtr pBody;
        private IntPtr pBorrowed;

        public Patcher(MethodBase method, MethodInfo replacement) {
            if(!IsSupported())
                throw new PlatformNotSupportedException("Not supported on non x86_x64 processors");

            this.method = method;
            this.replacement = replacement;
        }

        public static bool IsSupported() {
            if(FullscreenUtility.IsMacOS) return false;
            // Does not work on ARM/M1 macs
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(SystemInfo.processorType, "ARM", CompareOptions.IgnoreCase) == -1 && Environment.Is64BitProcess;
        }

        public bool IsPatched() {
            // var cursor = (byte * )pBody.ToPointer();
            // var isOriginal = backup.All(b => * (cursor++) == b); 

            return swapped;
        }

        public void Revert() {
            if(!swapped) {
                throw new Exception("Methods is not patched");
            }
            swapped = false;

            unsafe {
                var cursor = (byte*)pBody.ToPointer();
                for(var i = 0; i < backup.Length; i++) {
                    *(cursor++) = backup[i];
                }
            }
        }

        public void InvokeOriginal(object obj, params object[] parameters) {
            try {
                if(IsPatched())
                    Revert();
                method.Invoke(obj, parameters);
            } finally {
                if(!IsPatched())
                    SwapMethods();
            }
        }

        public void SwapMethods() {
            if(swapped) {
                throw new Exception("Methods already patched");
            }
            swapped = true;

            RuntimeHelpers.PrepareMethod(method.MethodHandle);
            RuntimeHelpers.PrepareMethod(replacement.MethodHandle);

            pBody = method.MethodHandle.GetFunctionPointer();
            pBorrowed = replacement.MethodHandle.GetFunctionPointer();

            unsafe {

                var ptr = (byte*)pBody.ToPointer();
                var ptr2 = (byte*)pBorrowed.ToPointer();
                var ptrDiff = ptr2 - ptr - 5;
                var relativeJumpAvailable = ptrDiff < (long)0xFFFFFFFF && ptrDiff > (long)-0xFFFFFFFF;
                var doNotUseRelativeJump = true; // See issues #69 and #89

                Logger.Debug("Relative jump is {0}available \\ {1}bit platform", relativeJumpAvailable ? "" : "not ", sizeof(IntPtr) * 8);

                // Backup orignal opcodes so we can revert it later
                for(var i = 0; i < backup.Length; i++) {
                    backup[i] = *(ptr + i);
                }

                if(!doNotUseRelativeJump && relativeJumpAvailable) {
                    // 32-bit relative jump, available on both 32 and 64 bit arch.
                    // Debug.Trace($"diff is {ptrDiff} doing relative jmp");
                    // Debug.Trace("patching on {0:X}, target: {1:X}", (ulong)ptr, (ulong)ptr2);
                    *ptr = 0xE9; // JMP
                    *((uint*)(ptr + 1)) = (uint)ptrDiff;
                } else {
                    // Debug.Trace($"diff is {ptrDiff} doing push+ret trampoline");
                    // Debug.Trace("patching on {0:X}, target: {1:X}", (ulong)ptr, (ulong)ptr2);
                    if(sizeof(IntPtr) == 8) {
                        // For 64bit arch and likely 64bit pointers, do:
                        // PUSH bits 0 - 32 of addr
                        // MOV [RSP+4] bits 32 - 64 of addr
                        // RET
                        var cursor = ptr;
                        *(cursor++) = 0x68; // PUSH
                        *((uint*)cursor) = (uint)ptr2;
                        cursor += 4;
                        *(cursor++) = 0xC7; // MOV [RSP+4]
                        *(cursor++) = 0x44;
                        *(cursor++) = 0x24;
                        *(cursor++) = 0x04;
                        *((uint*)cursor) = (uint)((ulong)ptr2 >> 32);
                        cursor += 4;
                        *(cursor++) = 0xC3; // RET
                    } else {
                        // For 32bit arch and 32bit pointers, do: PUSH addr, RET.
                        *ptr = 0x68;
                        *((uint*)(ptr + 1)) = (uint)ptr2;
                        *(ptr + 5) = 0xC3;
                    }
                }

                // Logger.Debug("Patched 0x{0:X} to 0x{1:X}.", (ulong)ptr, (ulong)ptr2);
            }
        }

    }
}
