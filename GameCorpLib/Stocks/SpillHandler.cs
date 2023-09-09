using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public interface ISpillHandler<TResourceType> where TResourceType : IResource
	{
		void HandleSpill(R<TResourceType> spillAmount);
	}

	public interface IUnderfillHandler<TResourceType> where TResourceType : IResource
	{
		void HandleUnderfill(R<TResourceType> underfillAmount);
	}

	public class BasicSpillHandler<TResourceType> : ISpillHandler<TResourceType>, IUnderfillHandler<TResourceType> where TResourceType : IResource
	{
		R<TResourceType> spillAmount;
		public void HandleSpill(R<TResourceType> spillAmount)
		{
			this.spillAmount += spillAmount;
		}
		public void HandleUnderfill(R<TResourceType> underfillAmount)
		{
			this.spillAmount -= underfillAmount;
		}
	}
	public class BankMoneySpillHandler : ISpillHandler<Money>, IUnderfillHandler<Money>
	{

		Bank _bank;
		Player _player;
		public BankMoneySpillHandler(Bank bank, Player player)
		{
			_bank = bank;
			_player = player;
		}

		public void HandleSpill(R<Money> spillAmount)
		{
			//Should never happen
			throw new NotImplementedException();
		}
		public void HandleUnderfill(R<Money> underfillAmount)
		{
			lock (_player.Stock) {
				_bank.TakeLoan(_player, underfillAmount);
				_player.Stock.ForceIncreaseResources<Money>(-underfillAmount);
			}
		}
	}

	public class ResourceSpillHandler<TResourceType> : ISpillHandler<TResourceType>, IUnderfillHandler<TResourceType> where TResourceType : IResource
	{
		SpotMarketInResource<TResourceType> _spotMarket;
		Player _player;

		public ResourceSpillHandler(Player player, SpotMarketInResource<TResourceType> spotMarket)
		{
			_spotMarket = spotMarket;
			_player = player;
		}
		public void HandleSpill(R<TResourceType> spillAmount)
		{
			_spotMarket.OnMarketPriceLiqudation(spillAmount, _player);
		}
		public void HandleUnderfill(R<TResourceType> underfillAmount)
		{
			//Should never happen
			throw new NotImplementedException();
		}
	}
}
