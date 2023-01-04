//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2023 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.Serialization;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.QueryView;
using ActiveQueryBuilder.View.WPF.Serialization;

namespace GeneralAssembly
{
    public class Options
    {
        public BehaviorOptions BehaviorOptions { get; set; }
        public DatabaseSchemaViewOptions DatabaseSchemaViewOptions { get; set; }
        public DesignPaneOptions DesignPaneOptions { get; set; }
        public VisualOptions VisualOptions { get; set; }
        public AddObjectDialogOptions AddObjectDialogOptions { get; set; }
        public DataSourceOptions DataSourceOptions { get; set; }
        public QueryNavBarOptions QueryNavBarOptions { get; set; }
        public UserInterfaceOptions UserInterfaceOptions { get; set; }
        public SQLFormattingOptions SqlFormattingOptions { get; set; }
        public SQLGenerationOptions SqlGenerationOptions { get; set; }

        private readonly List<OptionsBase> _options = new List<OptionsBase>();

        public Options()
        {
            CreateDefaultOptions();
        }

        public void CreateDefaultOptions()
        {
            BehaviorOptions = new BehaviorOptions();
            DatabaseSchemaViewOptions = new DatabaseSchemaViewOptions();
            DesignPaneOptions = new DesignPaneOptions();
            VisualOptions = new VisualOptions();
            AddObjectDialogOptions = new AddObjectDialogOptions();
            DataSourceOptions = new DataSourceOptions();
            QueryNavBarOptions = new QueryNavBarOptions();
            UserInterfaceOptions = new UserInterfaceOptions();
            SqlFormattingOptions = new SQLFormattingOptions();
            SqlGenerationOptions = new SQLGenerationOptions();
        }

        private void InitializeOptionsList()
        {
            _options.Clear();
            _options.Add(BehaviorOptions);
            _options.Add(DatabaseSchemaViewOptions);
            _options.Add(DesignPaneOptions);
            _options.Add(VisualOptions);
            _options.Add(AddObjectDialogOptions);
            _options.Add(DataSourceOptions);
            _options.Add(QueryNavBarOptions);
            _options.Add(UserInterfaceOptions);
            _options.Add(SqlFormattingOptions);
            _options.Add(SqlGenerationOptions);
        }

        public string SerializeToString()
        {
            InitializeOptionsList();

            var result = string.Empty;
            using (var stream = new MemoryStream())
            {
                using (var xmlBuilder = new XmlDescriptionBuilder(stream))
                {
                    var service = new OptionsSerializationService(xmlBuilder) {SerializeDefaultValues = true};
                    XmlSerializerExtensions.Builder = xmlBuilder;
                    using (IDisposable root = xmlBuilder.BeginObject("Options"))
                    {
                        foreach (var option in _options)
                        {
                            using (var optionHandle = xmlBuilder.BeginObjectProperty(root, option.GetType().Name))
                            {
                                service.EncodeObject(optionHandle, option);
                            }
                        }
                    }
                }

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            return result;
        }

        public void SerializeToFile(string path)
        {
            InitializeOptionsList();
            
            File.WriteAllText(path, SerializeToString());
        }

        public void DeserializeFromFile(string path)
        {
            InitializeOptionsList();
            
            DeserializeFromString(File.ReadAllText(path));
        }

        public void DeserializeFromString(string xml)
        {
            InitializeOptionsList();

            var buffer = Encoding.UTF8.GetBytes(xml);
            using (var memoryStream = new MemoryStream(buffer))
            {
                var adapter = new XmlAdapter(memoryStream);
                var service = new OptionsDeserializationService(adapter);
                XmlSerializerExtensions.Adapter = adapter;

                adapter.Reader.ReadToFollowing(_options[0].GetType().Name);

                foreach (var option in _options)
                {
                    var optionTree = adapter.Reader.ReadSubtree();
                    optionTree.Read();
                    service.DecodeObject(optionTree, option);
                    optionTree.Close();
                    adapter.Reader.Read();
                }
            }
        }
    }
}
