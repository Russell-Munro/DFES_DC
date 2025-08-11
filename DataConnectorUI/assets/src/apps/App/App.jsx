import React, { useState } from "react";

import { HashRouter as Router, Switch, Route } from "react-router-dom";

//ui and styles
import { Container, makeStyles } from "@material-ui/core";
// import styles from "./App.styles";

//components
import TopMenu from "../../components/Layout/TopMenu/TopMenu";
import Connections from "../../components/Connections";
import Rules from "../Containers/Rules/Rules";
import { ApolloProvider } from "@apollo/react-hooks";

import ApolloClient, { gql, InMemoryCache } from "apollo-boost";
import MappingContainer from "../Containers/MappingContainer/MappingContainer";
import ConnectionForm from "../Containers/ConnectionForm";
import Logs from "../../components/Logs/Logs";
import Log from "../../components/Logs/Log";

const client = new ApolloClient({
  cache: new InMemoryCache(),
  shouldBatch: true,
  resolvers: {
    Query: {
      messageCount: (_root, variables, { cache, getCacheKey }) => {
        const id = getCacheKey({ __typename: "ConnectionType", id: "2" });
        const fragment = gql`
          fragment connectionWhole on ConnectionType {
            id
            name
            destinationPlatformCfg
            sourcePlatformCfg
            enabled
            dateCreated
            connectionRules {
              id
              name
              syncIntervalCron
            }
          }
        `;

        const connection = cache.readFragment({ fragment, id });
        console.log(connection);
        return connection;
      }
    },
    Mutation: {
      updateCurrentRule: (_root, variables, { cache, getCacheKey }) => {
        // console.log(variables)
        const id = getCacheKey({
          __typename: "ConnectionRuleType",
          id: variables.id
        });
        const fragment = gql`
          fragment updateCurrentRule on ConnectionRuleType {
            id
            name
            enabled
          }
        `;

        const connectionRule = cache.readFragment({ fragment, id });
        const data = { ...connectionRule, ...variables.data };
        cache.writeData({ id, data });
        return null;
      }
    }
  }
});

client
  .query({
    query: gql`
      {
        connections {
          id
          name
          destinationPlatformCfg
          sourcePlatformCfg
          enabled
          dateCreated
          connectionRules {
            name
            id
          }
        }
      }
    `
  })
  .then(result => console.log(result));

const useStyles = makeStyles(theme => ({
  root: {
    marginTop: theme.spacing(20),
    backgroundColor: "#f5f5f5"
  }
}));
const websocket = new WebSocket(window.schedulingAgentWebSocket);


function App(props) {
  const classes = useStyles();
  const [webSocketState, setWebsocketState] = useState();

  websocket.onopen = function(evt) {
    //console.log(evt);
    setWebsocketState(websocket.readyState);
    console.log(`socket open`);
  };

  websocket.onclose = function(evt) {
    //console.log(evt);
    setWebsocketState(websocket.readyState);
    console.log(`socket closed`);
  };

  return (
    <ApolloProvider client={client}>
      <Router>
        <TopMenu websocket={websocket} webSocketState={webSocketState} />
        <Container maxWidth="lg" className={classes.root}>
          <Switch>
            <Route
              path="/logs/:connectionRuleId/:logId"
              render={routeProps => <Log {...routeProps} />}
            ></Route>
            <Route
              path="/logs/:connectionRuleId"
              render={routeProps => <Logs {...routeProps} />}
            ></Route>
            <Route
              path="/logs"
              render={routeProps => <Logs {...routeProps} />}
            ></Route>

            <Route
              path="/connections/:connectionId/:ruleId"
              render={routeProps => <MappingContainer {...routeProps} websocket={websocket} />}
            ></Route>
            <Route
              path="/connection/new"
              render={routeProps => <ConnectionForm {...routeProps} />}
            ></Route>
            <Route
              path="/connection/:connectionId/edit"
              render={routeProps => <ConnectionForm {...routeProps} />}
            ></Route>

            <Route
              path="/connections/:connectionId"
              render={routeProps => (
                <Rules
                  {...routeProps}
                  websocket={websocket}
                  webSocketState={webSocketState}
                />
              )}
            ></Route>

            <Route
              path="/connections"
              render={routeProps => <Connections />}
            ></Route>

            <Route path="/" render={routeProps => <Connections />}></Route>
          </Switch>
        </Container>
      </Router>
    </ApolloProvider>
  );
}

export default App;
