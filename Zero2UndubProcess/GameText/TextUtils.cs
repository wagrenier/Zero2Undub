using System;

namespace Zero2UndubProcess.GameText
{
    public static class TextUtils
    {
        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            var result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }
    }
}