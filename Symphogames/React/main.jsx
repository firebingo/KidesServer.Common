import React from "react";
import ReactDOM from "react-dom";
import { renderToStaticMarkup } from "react-dom/server";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import IndexView from "./js/index-view/index-view.jsx"
import GameView from "./js/game-view/game-view.jsx"

class Main extends React.Component {
	constructor(props) {
		super(props);
        this.state = {
            isLoaded: true
		};
	}

    componentDidMount() { }

	render() {
		const { isLoaded } = this.state;
        if (isLoaded) {
            return (
                <BrowserRouter>
					<Switch>
						<Route exact path="/" component={IndexView} />
                        <Route exact path="/game" component={GameView} />
					</Switch>
				</BrowserRouter>);
		} else {
			return <div className="centered">Loading...</div>
		}
    }
}

export default Main;