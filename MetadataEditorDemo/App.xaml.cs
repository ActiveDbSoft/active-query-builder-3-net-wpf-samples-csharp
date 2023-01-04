//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2023 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using ActiveQueryBuilder.Core;

namespace MetadataEditorDemo
{
    public partial class App : Application
    {
        public App()
        {
            // register metadata providers
            MSSQLConnectionDescriptor.Register();
            ODBCConnectionDescriptor.Register();
            OLEDBConnectionDescriptor.Register();
            MySQLConnectionDescriptor.Register();
            FirebirdConnectionDescriptor.Register();
            SybaseConnectionDescriptor.Register();
            PostgreSQLConnectionDescriptor.Register();
        }
    }
}
