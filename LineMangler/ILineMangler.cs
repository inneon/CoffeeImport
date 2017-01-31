using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommunityCoffeeImport.LineMangler
{
	interface ILineMangler
	{
		string Mangle(string line);
	}
}
