//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;

//namespace Madingley
//{
//    class TextualOutput
//    {
//        // An enumeration to hold the detail level for textual output
//        private enum TextDetailLevel { Low, Medium, High };
//        TextDetailLevel ModelDetailLevel;

//        /// <summary>
//        /// Constructor for TextualOutput
//        /// </summary>
//        /// <param name="textDetail">A string with one of three values (Low, Medium, or High) specifying the level of textual output requested</param>
//        public TextualOutput(string textDetail)
//        {
//            if (textDetail.ToLower() == "low")
//            {
//                ModelDetailLevel = TextDetailLevel.Low;
//            }
//            else if (textDetail.ToLower() == "medium")
//            {
//                ModelDetailLevel = TextDetailLevel.Medium;
//            }
//            else if (textDetail.ToLower() == "high")
//            {
//                ModelDetailLevel = TextDetailLevel.High;
//            }
//            else
//            {
//                Debug.Fail("Specification for model textual output detail level in model initialisation file is not one of the allowable values of 'Low', 'Medium' or 'High'");
//            }
//        }

//        public void WriteOutput(string stringToAdd, string detailLevel, ConsoleColor colourToUse)
//        {
//            if (detailLevel.ToLower() == "low") 
//            {
//                Console.ForegroundColor = colourToUse;
//                Console.WriteLine(stringToAdd);
//            }
//            else if (detailLevel.ToLower() == "medium")
//            {
//                if (ModelDetailLevel != TextDetailLevel.Low)
//                {
//                    Console.ForegroundColor = colourToUse;
//                    Console.WriteLine(stringToAdd);
//                }
//            }
//            else if (detailLevel.ToLower() == "high")
//            {
//                if (ModelDetailLevel == TextDetailLevel.High)
//                {
//                    Console.ForegroundColor = colourToUse;
//                    Console.WriteLine(stringToAdd);
//                }
//            }
//            else
//            {
//                Debug.Fail("Specification for model textual output detail level when adding initial output is not one of the allowable values of 'Low', 'Medium' or 'High'");
//            }
//        }     
  
//    }
//}
