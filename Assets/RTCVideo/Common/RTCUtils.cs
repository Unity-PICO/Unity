using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

namespace bytertc
{
    public class RTCUtils
    {
        public static IntPtr ArrayToIntPtr<T>(T[] myStructs)
        {
           // int size = Marshal.SizeOf(array[0])*array.Length;
             // ����ṹ���С���ֽڣ�
            int sizeOfMyStruct = Marshal.SizeOf<T>();
        
            // �����ڴ�ռ�
            IntPtr ptr = Marshal.AllocHGlobal(sizeOfMyStruct * myStructs.Length);
        
            try
            {
                // ������ƽṹ�嵽ָ��λ��
                for (int i = 0; i < myStructs.Length; i++)
                {
                    IntPtr currentPtr = new IntPtr((long)ptr + i * sizeOfMyStruct);
                
                    Marshal.StructureToPtr(myStructs[i], currentPtr, false);
                }
            
                Debug.Log("success transform");
            }
            catch (Exception e)
            {
                Debug.LogError($"transform error��{e}");
            }
     
            return ptr;
        }
        public static IntPtr StructToIntPtr<T>(T info)
        {
            int size = Marshal.SizeOf(info);
            IntPtr intPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(info, intPtr, false);
            }catch
            {
                Marshal.FreeHGlobal(intPtr);
            }
            return intPtr;
        }
      
    }
}
