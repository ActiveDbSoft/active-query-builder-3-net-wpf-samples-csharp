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
using System.Linq;
using System.Text;

namespace FullFeaturedDemo.Connection
{
    public delegate void SyntaxProviderDetected(Type syntaxType);

    interface IConnectionFrame
    {        
        event SyntaxProviderDetected OnSyntaxProviderDetected;
        void SetServerType(string serverType);        

        string ConnectionString { set; get; }
        bool TestConnection();
    }
}
