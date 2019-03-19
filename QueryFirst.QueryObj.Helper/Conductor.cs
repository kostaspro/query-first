using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using TinyIoC;

namespace QueryFirst.QueryObj.Helper
{
    public class Conductor : IConductor
    {
        private TinyIoCContainer _tiny;
        private VSOutputWindow _vsOutputWindow;
        private ICodeGenerationContext _ctx;

        public QFConfigModel Config => (QueryFirst.QueryObj.Helper.QFConfigModel)this._ctx.Config;

        public void ProcessOneQuery(Document queryDoc)
        {
            try
            {
                _ctx.InitForQuery(queryDoc);

                if (string.IsNullOrEmpty(_ctx.Config.DefaultConnection))
                {
                    _vsOutputWindow.Write(@"QueryFirst would like to help you, but you need to tell it where your DB is.
Breaking change in 1.0.0: QueryFirst now has it's own config file. You need to create qfconfig.json beside or above your query 
or put --QfDefaultConnection=myConnectionString somewhere in your query file.
See the Readme section at https://marketplace.visualstudio.com/items?itemName=bbsimonbb.QueryFirst    
");
                    return; // nothing to be done

                }
                if (!_tiny.CanResolve<IProvider>(_ctx.Config.Provider))
                {
                    _vsOutputWindow.Write(string.Format(
                        @"No Implementation of IProvider for providerName {0}. 
The query {1} may not run and the wrapper has not been regenerated.",
                        _ctx.Config.Provider, _ctx.BaseName
                    ));
                    return;
                }

                // also called in the bowels of schema fetching, for Postgres, because no notion of declarations.
                try
                {
                    var undeclared = _ctx.Provider.FindUndeclaredParameters(_ctx.Query.Text, _ctx.Config.DefaultConnection);
                    var newParamDeclarations = _ctx.Provider.ConstructParameterDeclarations(undeclared);
                    if (!string.IsNullOrEmpty(newParamDeclarations))
                    {
                        _ctx.Query.ReplacePattern("-- endDesignTime", newParamDeclarations + "-- endDesignTime");
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("sp_describe_undeclared_parameters"))
                        _vsOutputWindow.Write("Unable to find undeclared parameters. You will have to do this yourself.\n");
                    else throw;
                }

                _ctx.ResultFields = _ctx.SchemaFetcher.GetFields(_ctx.Config.DefaultConnection, _ctx.Config.Provider, _ctx.Query.Text);
                var code = new WrapperClassMaker()
                {
                    CodeGenerationContext = _ctx,
                    QueryFirstInterfaceType = Config?.QueryFirstInterfaceType
                }.TransformText();
                _ctx.PutCodeHere.WriteAndFormat(code);
            }
            catch (Exception ex)
            {
                _vsOutputWindow.Write(ex.TellMeEverything());
            }
        }


        public Conductor(VSOutputWindow vsOutpuWindow, ICodeGenerationContext ctx)
        {
            _vsOutputWindow = vsOutpuWindow;
            _ctx = ctx;
            _tiny = TinyIoCContainer.Current;
        }
    }
}
