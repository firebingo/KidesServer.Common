import React from "react";
import ReactDOM from "react-dom";
import { withTranslation } from 'react-i18next';
import { gameEnums } from "../globals.js"

class TurnControls extends React.Component {
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
        const { t, i18n } = this.props;
		const { gameInfo, hasSubmittedTurn } = this.props;
        const { selectedAction } = this.state;
        const defendDisabled = this.getOtherActionDisabled("DEFEND");
        const waitDisabled = this.getOtherActionDisabled("WAIT");
		if (hasSubmittedTurn) {
			return (
				<div>
                    {t('TURN_SUBMIT_WAIT')}
				</div>
			);
        }
		return (
			<div>
                <form onSubmit={e => this.submitTurn(e)} className="flex-column turn-actions">
					<div className="move-grid">
						{/* Table is a 3x3 grid with empty center space */}
						<table>
							<tbody>
								<tr>
									<td><div id="move-nw"
                                        onClick={e => this.moveClicked(e, gameEnums.direction["NORTHWEST"])}
                                        className={this.getMoveClassName(gameEnums.direction["NORTHWEST"])}>{t('NW')}</div>
									</td>
									<td><div id="move-n"
                                        onClick={e => this.moveClicked(e, gameEnums.direction["NORTH"])}
                                        className={this.getMoveClassName(gameEnums.direction["NORTH"])}>{t('N')}</div>
									</td>
									<td><div id="move-ne"
                                        onClick={e => this.moveClicked(e, gameEnums.direction["NORTHEAST"])}
                                        className={this.getMoveClassName(gameEnums.direction["NORTHEAST"])}>{t('NE')}</div>
									</td>
								</tr>
								<tr>
									<td><div id="move-w"
                                        onClick={e => this.moveClicked(e, gameEnums.direction["WEST"])}
                                        className={this.getMoveClassName(gameEnums.direction["WEST"])}>{t('W')}</div>
									</td>
									<td>
									</td>
									<td><div id="move-e"
                                        onClick={e => this.moveClicked(e, gameEnums.direction["EAST"])}
                                        className={this.getMoveClassName(gameEnums.direction["EAST"])}>{t('E')}</div>
									</td>
								</tr>
								<tr>
									<td><div id="move-sw"
                                        onClick={e => this.moveClicked(e, gameEnums.direction["SOUTHWEST"])}
                                        className={this.getMoveClassName(gameEnums.direction["SOUTHWEST"])}>{t('SW')}</div>
									</td>
									<td><div id="move-s"
                                        onClick={e => this.moveClicked(e, gameEnums.direction["SOUTH"])}
                                        className={this.getMoveClassName(gameEnums.direction["SOUTH"])}>{t('S')}</div>
									</td>
									<td><div id="move-se"
                                        onClick={e => this.moveClicked(e, gameEnums.direction["SOUTHEAST"])}
                                        className={this.getMoveClassName(gameEnums.direction["SOUTHEAST"])}>{t('SE')}</div>
									</td>
								</tr>
							</tbody>
						</table>
					</div>
					<div className="flex-column">
                        <div>{t('ATTACK')}</div>
						{this.buildAttackList()}
                    </div>
                    <div>
                        <input type="radio" id="defend-radio"
                            checked={selectedAction.type === gameEnums.action["DEFEND"]}
                            onChange={e => this.otherActionClicked(e)}
                            disabled={defendDisabled} />
                        <label htmlFor="defend-radio">{t('DEFEND')}</label>
                    </div>
                    <div>
                        <input type="radio" id="wait-radio"
                            checked={selectedAction.type === gameEnums.action["WAIT"]}
                            onChange={e => this.otherActionClicked(e)}
                            disabled={waitDisabled} />
                        <label htmlFor="wait-radio">{t('WAIT')}</label>
                    </div>
                    <input type="submit" value={t('SUBMIT_TURN')} />
				</form>
			</div>
		);
	}

	buildAttackList() {
		const { gameInfo } = this.props;
        const { selectedAction } = this.state;
        if (!gameInfo.actionInfo || !gameInfo.actionInfo.some(x => x.type == gameEnums.action["ATTACK"])) {
            return (
                <div className="attack-option">
                    None
                </div>
            )
        }
		return (gameInfo.actionInfo.map((x) => {
			if (x.type !== gameEnums.action["ATTACK"])
				return;
			const player = gameInfo.playerInfo.players.find(y => y.id === x.target);
			if (!player)
				return;
            return (<div key={`attack-${player.id}`} className="attack-option">
                <input id={`attack-radio-${player.id}`} type="radio" value={x.target}
					checked={selectedAction.target === x.target}
                    onChange={e => attackPlayerClicked(e)} />
                <label htmlFor={`attack-radio-${player.id}`}>{`${player.name} (${player.position.x}, ${player.position.y})`}</label>
			</div>)
		}));
	}

	getMoveClassName(dir) {
		const { gameInfo } = this.props;
		const { selectedAction } = this.state;
        let className = "move-button";

        if (!gameInfo.actionInfo.some(x => x.type === gameEnums.action["MOVE"] && x.direction === dir)) {
            className += " disabled";
        }

		if (!selectedAction || selectedAction.type != gameEnums.action["MOVE"])
			return className;

		if (selectedAction.direction === dir) {
			className += " selected";
        }
        return className;
    }

    getOtherActionDisabled(target) {
        const { gameInfo } = this.props;
        let action = undefined;
        try {
            switch (target) {
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
            return false;
        }
        return true;
    }

    moveClicked(e, dir) {
		const { gameInfo } = this.props;
		const { selectedAction } = this.state;
		const action = gameInfo.actionInfo.find(x => x.type === gameEnums.action["MOVE"] && x.direction === dir);
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

	otherActionClicked(e) {
		const { gameInfo } = this.props;
        let action = null;
        try {
            switch (e.target.id) {
                case "defend-radio":
					action = gameInfo.actionInfo.find(x => x.type === gameEnums.action["DEFEND"])
					break;
				case "wait-radio":
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
            this.setState({
                selectedAction: {}
            });
		}
	}
}

export default withTranslation()(TurnControls);