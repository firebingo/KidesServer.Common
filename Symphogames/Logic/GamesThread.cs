using Symphogames.Helpers;
using Symphogames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Symphogames.Logic
{
	public class GamesThread
	{
		private Timer _timer;
		private readonly SGame _threadGame;

		public GamesThread(SGame game)
		{
			_threadGame = game;
		}

		public async Task StartThread()
		{
			_timer = new Timer((await SymphogamesConfig.GetConfig()).GameTickMs)
			{
				AutoReset = false,
				Enabled = false,
				SynchronizingObject = null
			};
			_timer.Elapsed += OnGameTick;
			_timer.Start();
		}

		private void OnGameTick(object sender, ElapsedEventArgs e)
		{
			if (_threadGame.Started)
			{
				if (_threadGame.AllTurnsSubmitted)
				{
					foreach (var action in _threadGame.Turns[_threadGame.CurrentTurn].Actions)
					{
						try
						{
							var player = _threadGame.GetPlayerById(action.Key);
							switch (action.Value.Type)
							{
								case SActionType.Move:
									DoPlayerMove(player, action.Value);
									break;
								case SActionType.Wait:
									player.Energy = Math.Min(player.MaxEnergy, player.Energy + 0.2);
									action.Value.Result = true;
									break;
								case SActionType.Defend:
									player.Energy = Math.Min(player.MaxEnergy, player.Energy + 0.1);
									action.Value.Result = true;
									break;
								case SActionType.Attack:
									DoPlayerAttack(player, action.Value);
									break;
							}
							player.HasSubmittedTurn = false;
						}
						catch (Exception ex)
						{
							ErrorLog.WriteError(ex);
							action.Value.Result = false;
						}
					}
					_threadGame.IncrementTurn();
				}
			}

			_timer.Start();
		}

		private void DoPlayerMove(SGamePlayer player, SAction action)
		{
			switch (action.Direction)
			{
				case SDirection.North:
					player.Position.Y--;
					break;
				case SDirection.NorthEast:
					player.Position.Y--;
					player.Position.X++;
					break;
				case SDirection.East:
					player.Position.X++;
					break;
				case SDirection.SouthEast:
					player.Position.Y++;
					player.Position.X++;
					break;
				case SDirection.South:
					player.Position.Y++;
					break;
				case SDirection.SouthWest:
					player.Position.Y++;
					player.Position.X--;
					break;
				case SDirection.West:
					player.Position.X--;
					break;
				case SDirection.NorthWest:
					player.Position.Y--;
					player.Position.X--;
					break;
				default:
					action.Result = false;
					return;
			}
			action.Result = true;
			player.Energy -= 0.1;
		}

		private void DoPlayerAttack(SGamePlayer player, SAction action)
		{
			var damage = 0.0;
			if (!action.Target.HasValue)
			{
				action.Result = false;
				return;
			}
			var target = _threadGame.GetPlayerById(action.Target.Value);
			if (target == null)
			{
				action.Result = false;
				return;
			}
			var actions = _threadGame.Turns[_threadGame.CurrentTurn].Actions;
			//If the target player moved we cant attack them
			if (actions.Any(x => x.Key == target.Player.Id && x.Value.Direction.HasValue))
			{
				action.Result = false;
				return;
			}
			if (actions.Any(x => x.Key == target.Player.Id &&
			(x.Value.Type == SActionType.Defend || //If the target player is defending
			(x.Value.Type == SActionType.Attack && x.Value.Target.Value == player.Player.Id)))) //if the target player is attacking this player
			{
				damage = 0.1;
				action.Result = true;
				return;
			}
			damage -= 0.2;
			action.Result = true;

			if (player.Energy < 0.0)
				damage /= 2;

			target.Health -= damage;
			if (target.Health <= 0.0)
			{
				target.State = SPlayerState.Dead;
				target.DeathTurn = _threadGame.CurrentTurn;
				player.Kills.Add(new SKillRecord(_threadGame.Id, player.Player.Id, target.Player.Id, _threadGame.CurrentTurn, ""));
			}

			player.Energy -= 0.1;
			return;
		}

		public void StopThread()
		{
			_timer.Enabled = false;
			_timer.Stop();
			_timer.Dispose();
		}
	}
}
