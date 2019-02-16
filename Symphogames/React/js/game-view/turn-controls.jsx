import React from "react";
import ReactDOM from "react-dom";
import { Translate } from "react-localize-redux";
import { gameEnums } from "../globals.js"

export class TurnControls extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			error: null,
			//actions: {},
			selectedAction: {}
		};
	}

	checkAvailableActions() {
		const { gameInfo } = this.props;
		let actions = {
			move: {
				[gameEnums.direction["NORTH"]]: false,
				[gameEnums.direction["NORTHEAST"]]: false,
				[gameEnums.direction["EAST"]]: false,
				[gameEnums.direction["SOUTHEAST"]]: false,
				[gameEnums.direction["SOUTH"]]: false,
				[gameEnums.direction["SOUTHWEST"]]: false,
				[gameEnums.direction["WEST"]]: false,
				[gameEnums.direction["NORTHWEST"]]: false
			},
			defend: false,
			wait: false,
			attack: []
		};
		for (const a of gameInfo.actionInfo) {
			try {
				switch (a.type) {
					case gameEnums.action["MOVE"]:
						switch (a.direction) {
							case gameEnums.direction["NORTH"]:
								actions.move[gameEnums.direction["NORTH"]] = true;
								break;
							case gameEnums.direction["NORTHEAST"]:
								actions.move[gameEnums.direction["NORTHEAST"]] = true;
								break;
							case gameEnums.direction["EAST"]:
								actions.move[gameEnums.direction["EAST"]] = true;
								break;
							case gameEnums.direction["SOUTHEAST"]:
								actions.move[gameEnums.direction["SOUTHEAST"]] = true;
								break;
							case gameEnums.direction["SOUTH"]:
								actions.move[gameEnums.direction["SOUTH"]] = true;
								break;
							case gameEnums.direction["SOUTHWEST"]:
								actions.move[gameEnums.direction["SOUTHWEST"]] = true;
								break;
							case gameEnums.direction["WEST"]:
								actions.move[gameEnums.direction["WEST"]] = true;
								break;
							case gameEnums.direction["NORTHWEST"]:
								actions.move[gameEnums.direction["NORTHWEST"]] = true;
								break;
						}
						break;
					case gameEnums.action["ATTACK"]:
						const player = gameInfo.PlayerInfo.players.find(x => x.id === a.target);
						if (player) {
							actions.attack.push({ player });
						}
						break;
					case gameEnums.action["DEFEND"]:
						actions.defend = true;
						break;
					case gameEnums.action["WAIT"]:
						actions.wait = true;
						break;
				}
			} catch (error) {
				if (a) {
					console.log(`Error in action parse, type: ${a.type}, target: ${a.target}, error: ${error}`);
				} else {
					console.log(`Error in action parse, no action, error: ${error}`);
				}
			}
		}
		this.setState({
			actions: actions
		});
	}

	componentDidMount() {
		//checkAvailableActions();
	}

	render() {
		const { gameInfo, hasSubmittedTurn } = this.props;
		const { selectedAction } = this.state;
		if (hasSubmittedTurn) {
			return (
				<div>
					<Translate>
						{translate => (
							translate('TURN_SUBMIT_WAIT')
						)}
					</Translate>
				</div>
			);
		}
		return (
			<div>
				<Translate>
					{translate => (
						<form onSubmit={e => this.submitTurn(e)} className="flex-column turn-actions">
							<div className="move-grid">
								{/* Table is a 3x3 grid with empty center space */}
								<table>
									<tbody>
										<tr>
											<td><div id="move-nw"
												onClick={e => this.moveClicked(e, gameEnums.direction["NORTHWEST"])}
												className={this.getMoveClassName(gameEnums.direction["NORTHWEST"])}>{translate('NW')}</div>
											</td>
											<td><div id="move-n"
												onClick={e => this.moveClicked(e, gameEnums.direction["NORTH"])}
												className={this.getMoveClassName(gameEnums.direction["NORTH"])}>{translate('N')}</div>
											</td>
											<td><div id="move-ne"
												onClick={e => this.moveClicked(e, gameEnums.direction["NORTHEAST"])}
												className={this.getMoveClassName(gameEnums.direction["NORTHEAST"])}>{translate('NE')}</div>
											</td>
										</tr>
										<tr>
											<td><div id="move-w"
												onClick={e => this.moveClicked(e, gameEnums.direction["WEST"])}
												className={this.getMoveClassName(gameEnums.direction["WEST"])}>{translate('W')}</div>
											</td>
											<td>
											</td>
											<td><div id="move-e"
												onClick={e => this.moveClicked(e, gameEnums.direction["EAST"])}
												className={this.getMoveClassName(gameEnums.direction["EAST"])}>{translate('E')}</div>
											</td>
										</tr>
										<tr>
											<td><div id="move-sw"
												onClick={e => this.moveClicked(e, gameEnums.direction["SOUTHWEST"])}
												className={this.getMoveClassName(gameEnums.direction["SOUTHWEST"])}>{translate('SW')}</div>
											</td>
											<td><div id="move-s"
												onClick={e => this.moveClicked(e, gameEnums.direction["SOUTH"])}
												className={this.getMoveClassName(gameEnums.direction["SOUTH"])}>{translate('S')}</div>
											</td>
											<td><div id="move-se"
												onClick={e => this.moveClicked(e, gameEnums.direction["SOUTHEAST"])}
												className={this.getMoveClassName(gameEnums.direction["SOUTHEAST"])}>{translate('SE')}</div>
											</td>
										</tr>
									</tbody>
								</table>
							</div>
							<div className="flex-column">
								<div>{translate("ATTACK")}</div>
								{this.buildAttackList()}
							</div>
							<input type="radio" value="DEFEND"
								checked={selectedAction.type === gameEnums.action["DEFEND"]}
								onChange={e => this.otherActionClicked(e)}>{translate('DEFEND')}>{translate('DEFEND')}</input>
							<input type="radio" value="WAIT"
								checked={selectedAction.type === gameEnums.action["WAIT"]}
								onChange={e => this.otherActionClicked(e)}>{translate('WAIT')}>{translate('WAIT')}</input>
							<input type="submit" value={translate('SUBMIT_TURN')} />
						</form>
					)}
				</Translate>
			</div>
		);
	}

	buildAttackList() {
		const { gameInfo } = this.props;
		const { selectedAction } = this.state;
		return (gameInfo.actionInfo.map((x) => {
			if (x.type !== gameEnums.action["ATTACK"])
				return;
			const player = gameInfo.PlayerInfo.players.find(y => y.id === x.target);
			if (!player)
				return;
			return (<div className="attack-option">
				<input type="radio" value={x.target}
					checked={selectedAction.target === x.target}
					onChange={e => attackPlayerClicked(e)}>{`${player.name} (${player.position.x}, ${player.position.y})`}</input>
			</div>)
		}));
	}

	getMoveClassName(dir) {
		const { gameInfo } = this.props;
		const { selectedAction } = this.state;
		let className = "move-button";
		if (!selectedAction || selectedAction.type != gameEnums.action["MOVE"])
			return className;

		if (!gameInfo.actionInfo.some(x => x.type === gameEnums.action["MOVE"] && x.direction === gameEnums.direction[dir])) {
			className += " disabled";
			return;
		}

		if (selectedAction.direction === dir) {
			className += " selected";
		}
	}

	moveClicked(e, dir) {
		const { gameInfo } = this.props;
		const { selectedAction } = this.state;
		const action = gameInfo.actionInfo.find(x => x.type === gameEnums.action["MOVE"] && x.direction === gameEnums.direction[dir]);
		if (action && (selectedAction.direction !== action.direction)) {
			this.setState({
				selectedAction: action
			});
		}
	}

	attackPlayerClicked(e) {
		const { gameInfo } = this.props;
		let action = null;
		try {
			action = gameInfo.actionInfo.find(x => x.type === gameEnums.action["ATTACK"] && x.target === (+e.target.value))
		} catch (error) {
			console.log(`Unknown error, error: ${error}`);
		}
		if (action) {
			this.setState({
				selectedAction: action
			});
		}
	}

	otherActionChanged(e) {
		const { gameInfo } = this.props;
		let action = null;
		try {
			switch (e.target.value) {
				case "DEFEND":
					action = gameInfo.actionInfo.find(x => x.type === gameEnums.action["DEFEND"])
					break;
				case "WAIT":
					action = gameInfo.actionInfo.find(x => x.type === gameEnums.action["WAIT"])
					break;
			}
		} catch (error) {
			console.log(`Unknown error, error: ${error}`);
		}
		if (action) {
			this.setState({
				selectedAction: action
			});
		}
	}

	submitTurn(e) {
		if (this.props.submitTurn) {
			this.props.submitTurn(e, this.state.selectedAction);
		}
	}
}