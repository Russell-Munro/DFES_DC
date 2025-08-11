import React, { useState } from "react";

import { useQuery, useMutation } from "@apollo/react-hooks";
import { gql } from "apollo-boost";

import Connection from "../Connection/Connection";
import AddIcon from "@material-ui/icons/Add";
import { useHistory } from "react-router-dom";

import {
  Typography,
  Fab,
  Grid,
  CircularProgress,
  Paper,
  Fade,
  Breadcrumbs,
  Link
} from "@material-ui/core";
import { makeStyles } from "@material-ui/styles";
import Loader from "../Loader/Loader";
import GlobalToast from "../GlobalToast/GlobalToast";

const CONNECTIONS = gql`
  query getConnections {
    connections {
      id
      name
      enabled
      jsonDestinationPlatformCfg {
        endPointURL
        integratorID
        platformID
        serviceDomain
        servicePassword
        serviceUsername      
      }
      jsonSourcePlatformCfg {
        endPointURL
        integratorID
        platformID
        serviceDomain
        servicePassword
        serviceUsername      
      }
    }
  }
`;
const ADD_CONNECTION = gql`
  mutation {
    createConnection {
      name
      id
    }
  }
`;

const UPDATE_CONNECTION = gql`
  mutation($connection: connectionInput!) {
    updateConnection(connection: $connection) {
      id
      enabled
    }
  }
`;

const DELETE_CONNECTION = gql`
  mutation($connectionInput: connectionInput!) {
    deleteConnection(connection: $connectionInput) {
      name
    }
  }
`;

const useStyles = makeStyles(theme => ({
  connection: {
    borderBottom: "1px solid rgba(0,0,0,0.12)"
  },
  paper: {
    width: "100%",
    marginTop: theme.spacing(4),
    position: "relative"
  },
  gridHeader: {
    padding: theme.spacing(4),
    borderBottom: "1px solid rgba(0,0,0,0.12)",
    fontWeight: "Bold"
  },
  fab: {
    margin: theme.spacing(1),
    position: "absolute",
    bottom: "-24px",
    right: "50%"
  },
  pending: {
    margin: theme.spacing(10)
  },
  contentWrapper: {
    marginTop: theme.spacing(4),
    position: "relative"
  },
  icon:{
    color:"white"
  }
}));

function Connections(props) {
  const classes = useStyles();
  const user = window.userInfo;

  const [toastMesage, setToastMesage] = useState("");
  const [toastStatus, setToastStatus] = useState("info");
  const [openToast, setOpenToast] = React.useState(false);

  const [modalIsOpen, setModalIsOpen] = useState(false);

  let history = useHistory();

  const [createConnection] = useMutation(ADD_CONNECTION, {
    // update(cache, { data }) {
    //   //   console.log(data.createConnection);
    //   const cacheValue = cache.readQuery({ query: CONNECTIONS });
    //   // console.log(data.createConnection);
    //   cache.writeQuery({
    //     query: CONNECTIONS,
    //     data: { connections: cacheValue.connections.push(cacheValue) }
    //   });
    refetchQueries: ["getConnections"],
    onCompleted: data => {
      setToastStatus("success");
      setToastMesage(
        "New connection created. It must be configured before it can be used"
      );
      setOpenToast(true);
      // /history.push(`/connection/${createConnection.id}/edit`);
    }
  });

  const [updateConnection] = useMutation(UPDATE_CONNECTION, {
    refetchQueries: ["getConnections"],
    onCompleted: (data) => {

      var msg = (data.updateConnection.enabled)?"Enabled":"Disabled";

      // console.log(data);

      setToastStatus("success");
      setToastMesage(`Connection ${msg}`);
      setOpenToast(true);
      // /history.push(`/connection/${createConnection.id}/edit`);
    }
  });

  const [
    deleteConnection,
    { udeleteData, deleteLoading, deleteError, deleteCalled }
  ] = useMutation(DELETE_CONNECTION, {
    refetchQueries: ["getConnections"],
    onCompleted: data => {
      setToastStatus("success");
      setToastMesage("The connection has been removed from the database");
      setOpenToast(true);
    }
  });

  const handleDeleteConnection = id => {
    deleteConnection({
      variables: {
        connectionInput: { id: id }
      }
    });
  };

  // const handleNewConnection = () => {
  //   // console.log("clicked");
  //   // console.log(props);
  //   history.push("/connection/new");
  // };

  const RenderConnections = () => {
    return data.connections.map((connection, index) => {
      return (
        <Grid
          item
          xs={12}
          key={`grid-${connection.id}`}
          className={classes.connection}
        >
          <Connection
            key={`connection-${connection.id}`}
            {...connection}
            handleDelete={handleDeleteConnection}
            handleUpdate={updateConnection}
          />
        </Grid>
      );
    });
  };

  const { loading, error, data } = useQuery(CONNECTIONS);

  if (loading) {
    return <Loader />;
  }

  if (error) {
    // console.log(error)
    return <pre>{JSON.stringify(error, null, 1)}</pre>;
  }


  // console.log(data);

  return (
    <Grid container item xs={12} spacing={4} alignItems={"center"}>
      <Grid item xs={12}>
        <Breadcrumbs aria-label="breadcrumb">
          <Link color="inherit" href="/#/">
            Home
          </Link>

          <Typography color="textPrimary">Connections</Typography>
        </Breadcrumbs>
      </Grid>

      <Grid container item xs={12}>
        <Typography variant="h2">Content Sync Connections</Typography>
      </Grid>

      <Grid container item xs={12}>
        <Paper className={classes.paper}>
            <Grid container item xs={12} className={classes.gridHeader}>
                <Grid item xs={1} container justify={"center"}></Grid>
                <Grid item xs={8}>
                    <Typography variant={"subtitle2"}>Name</Typography>
                </Grid>
                <Grid item xs={2}>
                    <Typography variant={"subtitle2"}>Enabled</Typography>
                </Grid>
                <Grid item xs={1}></Grid>
            </Grid>

            <Grid container item xs={12}>
                <RenderConnections />
            </Grid>

          {user.IsConnectionAdmin && 
            <Fab
              color="primary"
              aria-label="add"
              size="small"
              className={classes.fab}
              onClick={createConnection}>
              <AddIcon  className={classes.icon}/>
            </Fab>
}
        </Paper>
      </Grid>

        


      
      <GlobalToast
        status={toastStatus}
        message={toastMesage}
        open={openToast}
        setOpenToast={setOpenToast}
      />
    </Grid>
  );
}

export default Connections;
