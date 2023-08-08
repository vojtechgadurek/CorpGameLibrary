using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib
{
	internal class Game
	{
		public int Round = 0;
		public Registers Registers = new Registers();

		public Game()
		{
			CreateAdminAccount();
		}
		void CreateAdminAccount()
		{
			new Player("admin", "admin", Registers.PlayersRegister);
		}
		public void NewRound()
		{
			Round++;
			Registers.PropertyRegister.Update();
		}
	}
}
