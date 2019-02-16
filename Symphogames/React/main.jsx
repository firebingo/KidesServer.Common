import React from "react";
import ReactDOM from "react-dom";
import { renderToStaticMarkup } from "react-dom/server";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import { withLocalize } from "react-localize-redux";
import IndexView from "./js/index-view/index-view.jsx"
import GameView from "./js/game-view/game-view.jsx"

class Main extends React.Component {
	constructor(props) {
		super(props);
        this.state = {
            isLoaded: true
		};
		this.props.initialize({
			languages: [
				{ name: "English", code: "en" },
			],
            options: {
                renderToStaticMarkup,
				defaultLanguage: "en"
			}
		});
	}

    componentDidMount() {
		//fetch(`/dist/translations/en.json`)
		//	.then(res => res.json())
		//	.then(
		//		(result) => {
		//			this.props.addTranslationForLanguage(result, "en");
		//			this.setState({ isLoaded: true });
		//		},
		//		(error) => {
		//			console.log("Failed to load language file");
		//			this.setState({ isLoaded: false });
		//		});
	}

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

export default withLocalize(Main);