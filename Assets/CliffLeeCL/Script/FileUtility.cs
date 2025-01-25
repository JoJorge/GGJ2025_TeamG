using System.IO;
using System;

namespace CliffLeeCL
{
    /// <summary>
    /// The class have some common functions that are related to file IO.
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        /// Write a generic array to a file.
        /// </summary>
        /// <param name="destinationPath">The path that file will be stored in.</param>
        /// <param name="arrayRow">Row number of the array.</param>
        /// <param name="arrayColumn">Column number of the array.</param>
        /// <param name="array">The source array.</param>
        public static void WriteArray(string destinationPath, int arrayRow, int arrayColumn, Array array)
        {
            string outputText = "";

            for (int i = 0; i < arrayRow; i++)
                for (int j = 0; j < arrayColumn; j++)
                    outputText += array.GetValue(i * arrayColumn + j).ToString() + ((j == arrayColumn - 1) ? "\n" : " ");

            File.WriteAllText(destinationPath, outputText);
        }
    }
}
