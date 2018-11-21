import React from "react";
import ReactDOM from "react-dom";
import { Translate } from "react-localize-redux";
import { Redirect } from "react-router-dom";

export class IndexView extends React.Component {
	constructor(props) {
		super(props);
		this.joinGame = this.joinGame.bind(this);
		this.handleInputChange = this.handleInputChange.bind(this);
		sessionStorage.removeItem("accessData");
		this.state = {
			userIdInput: '',
			gameIdInput: '',
			error: '',
			redirectToGame: false
		}
	}

	render() {
		const { userIdInput, gameIdInput, error, redirectToGame } = this.state;
		const errorSplit = (error && error.message) ? error.message.split("|") : [""];
		const InputFields = (
			<Translate>
				{translate => (
					<div className="flex-column">
						<input name="userIdInput" value={userIdInput} onChange={this.handleInputChange} placeholder={translate('PLAYER_ID')} required></input>
						<input name="gameIdInput" value={gameIdInput} onChange={this.handleInputChange} placeholder={translate('GAME_ID')} required></input>
						<input type="submit" value={translate('JOIN')} />
						{(errorSplit.length > 0 && errorSplit[0]) && <span className="error-text">{translate(errorSplit[0], { id: errorSplit[1] })}</span>}
					</div>
				)}
			</Translate>
		);
		if (redirectToGame) {
			return <Redirect to="/game" />;
		} else {
			return (
				<form className="centered" onSubmit={this.joinGame}>
					<InputFields />
				</form>);
		}
	}

	handleInputChange(ev) {
		const target = ev.target;
		const value = target.type === 'checkbox' ? target.checked : target.value;
		const name = target.name;

		this.setState({
			[name]: value
		});
	}

	joinGame(ev) {
		ev.preventDefault();
		const { userIdInput, gameIdInput } = this.state;
		fetch(`/api/v1/symphogames/join-game?gameId=${gameIdInput}&playerId=${userIdInput}`)
			.then(res => res.json())
			.then(
				(result) => {
					if (!result.success) {
						this.setState({
							error: { message: result.message }
						});
					} else {
						sessionStorage.setItem("accessData", JSON.stringify({
							gameId: result.gameId,
							playerId: result.playerId,
							accessGuid: result.accessGuid
						}));
						this.setState({
							redirectToGame: true
						});
					}
				},
				(error) => {
					this.setState({
						error
					});
				});
	}
}