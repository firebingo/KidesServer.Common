import React from "react";
import ReactDOM from "react-dom";
import "./less/main.less";
import Main from "./main.jsx"
import i18next  from "i18next";
import { initReactI18next } from "react-i18next";
import XHR from 'i18next-xhr-backend';


const App = props => (
	<Main />
);

function renderView() {
    ReactDOM.render(<App />, document.getElementById("content"));
}

i18next 
    .use(initReactI18next)
    .use(XHR)
    .init({
        backend: {
            loadPath: '/dist/translations/{{lng}}.json',
            parse: (data) => { return JSON.parse(data); }
        },
        lng: "en",
        fallbackLng: "en",
        load: "currentOnly",
        interpolation: {
            escapeValue: false
        }
    }, function (error, t) {
        renderView();
    });