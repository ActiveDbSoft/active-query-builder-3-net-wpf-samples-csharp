//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiveQueryBuilder.View;

namespace ExpressionEditorDemo.Common
{
    public static class Helpers
    {
        public static CFont ToCFont(object nativeFont)
        {
            var font = nativeFont as Font;
            if (font == null)
                return null;

            return new CFont(font.FontFamily.Name, font.Size, font.Bold, font.Italic)
            {
                LineSpacing = font.Height / font.Size
            };
        }
    }
}
