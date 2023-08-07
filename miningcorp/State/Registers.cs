using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.State
{
	public class Registers
	{
		public PropertyRegister PropertyRegister = new PropertyRegister();
		public PlayersRegister PlayersRegister = new PlayersRegister();
		public CompanyRegister CompanyRegister = new CompanyRegister();
	}
}
