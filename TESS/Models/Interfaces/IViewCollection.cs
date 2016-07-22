using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TietoCRM.Models.Interfaces
{
    public interface IViewCollection
    {
        List<SQLBaseClass> GetViews(String condition);
    }
}
