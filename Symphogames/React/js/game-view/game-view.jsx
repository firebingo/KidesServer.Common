import React from "react";
import ReactDOM from "react-dom";
import { Redirect } from "react-router-dom";
import { withTranslation } from 'react-i18next';
import Map from "./map.jsx";
import TurnControls from "./turn-controls.jsx";
import { setInterval, clearTimeout } from "timers";

class GameView extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			error: null,
			isLoaded: false,
			gameInfo: {},
			hasSubmittedTurn: false,
			redirectToLogin: false
        };
        this.submitTurn = this.submitTurn.bind(this);
        this.updateGameInfo = this.updateGameInfo.bind(this);
        this.renderGameInfo = this.renderGameInfo.bind(this);
        //TODO: Eventually try to get rid of intervals with signalr
        this.updateTimeout = undefined;
	}

    componentDidMount() {
        this.updateGameInfo();
    }

    componentWillUnmount() {
        if (this.updateTimeout) {
            clearTimeout(this.updateTimeout);
            this.updateTimeout = undefined;
        }
    }

    render() {
        const { t, i18n } = this.props;
		const { error, isLoaded, gameInfo, redirectToLogin } = this.state;
		if (redirectToLogin) {
			return <Redirect to="/" />;
		} else {
			if (error) {
				return (
					<div className="centered">{t('ERROR', { msg: error.message })}</div>
				);
			} else if (!isLoaded) {
				return (
					<div className="centered">{t('LOADING')}</div>
				);
			} else {
				return (
                    <div className="centered flex-column">
                        {this.renderGameInfo(gameInfo)}
                        <TurnControls gameInfo={gameInfo} hasSubmittedTurn={this.state.hasSubmittedTurn} submitTurn={this.submitTurn} />
						<Map gameInfo={gameInfo} />
					</div>
				);
			}
		}
	}

    renderGameInfo(gameInfo) {
        return (
            <div className="flex-column">
                <div>{`Turn #${gameInfo.gameInfo.currentTurn + 1}`}</div>
                <div>{`Health: ${gameInfo.playerInfo.thisPlayer.health * 100}`}</div>
                <div>{`Energy: ${gameInfo.playerInfo.thisPlayer.energy * 100}`}</div>
            </div>
        )
    }

    submitTurn(e, action) {
        e.preventDefault();
        const accessData = JSON.parse(sessionStorage.getItem("accessData"));
        fetch(`/api/v1/symphogames/submit-turn?gameId=${accessData.gameId}&playerId=${accessData.playerId}&accessGuid=${accessData.accessGuid}`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(action)
            })
            .then(res => res.json())
            .then(
                (result) => {
                    this.setState({ hasSubmittedTurn: true });
                },
                (error) => {

                });
    }

    updateGameInfo() {
        const accessData = JSON.parse(sessionStorage.getItem("accessData"));
        if (!accessData) {
            this.setState({
                redirectToLogin: true
            });
            return;
        }
        fetch(`/api/v1/symphogames/current-player-game-info?gameId=${accessData.gameId}&playerId=${accessData.playerId}&accessGuid=${accessData.accessGuid}`)
            .then(res => res.json())
            .then(
                (result) => {
                    this.setState({
                        hasSubmittedTurn: result.playerInfo.thisPlayer.hasSubmittedTurn,
                        isLoaded: true,
                        gameInfo: result
                    });

                    this.updateTimeout = setTimeout(this.updateGameInfo, 5000);
                },
                (error) => {
                    this.setState({
                        isLoaded: true,
                        error
                    });
                });
    }
}

export default withTranslation()(GameView);