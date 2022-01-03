using Holism.Generation;
using System;
using Holism.Infra;

namespace Holism.Generation
{
    public class ViewGenerator : Generator
    {
        public void CreateViews()
        {
            if (Database.Views == null)
            {
                return;
            }
            foreach(var view in Database.Views)
            {
                try
                {
                    CreateView(view);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error creating view {view.Name}");
                    Logger.LogException(ex);
                }
            }
        }

        public void CreateView(View view)
        {
            var query = @$"
                drop view if exists `{view.Name}`
            ";
            Holism.DataAccess.Database.Open(ConnectionString).Run(query);
            query = @$"
                CREATE view {view.Name}
                as
                {view.ConcatenatedQuery}
            ";
            Holism.DataAccess.Database.Open(ConnectionString).Run(query);
        }
    }
}