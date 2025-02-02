//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;


namespace GeneralAssembly.Connection
{
    public delegate void SyntaxProviderDetected(Type syntaxType);

    interface IConnectionFrame
    {        
        event SyntaxProviderDetected SyntaxProviderDetected;
        void SetServerType(string serverType);        

        string ConnectionString { set; get; }
        bool TestConnection();
    }
}
