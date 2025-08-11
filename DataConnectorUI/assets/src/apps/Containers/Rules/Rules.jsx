import React, { PureComponent, useState } from "react";

//components
import {
  Typography,
  Paper,
  Breadcrumbs,
  Link,
  makeStyles,
  Grid,
  Fab,
  Modal,
  Button
} from "@material-ui/core";

//styles
import AddIcon from "@material-ui/icons/Add";
import Rule from "../../../components/Rule/Rule";
import gql from "graphql-tag";
import { useQuery, useMutation } from "@apollo/react-hooks";
import Loader from "../../../components/Loader/Loader";
import { useHistory } from "react-router-dom";
import GlobalToast from "../../../components/GlobalToast/GlobalToast";

const useStyles = makeStyles(theme => ({
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
  pageTitle: {
    marginTop: theme.spacing(4),
    marginBottom: theme.spacing(4)
  },
  norules: {
    padding: theme.spacing(4)
  },

  fab: {
    float: "left",
    top: 20
  },
  icon: {
    color: "white"
  },
  log: {
    position: "absolute",
    width: "80%",
    height: "80%",
    top: "10%",
    bottom: "10%",
    left: "10%",
    right: "10%",
    border: 0
  }
}));

const LOGS = gql`
  query getLogs($connectionRuleId: ID, $pageSize: Int, $pageNo: Int) {
    logs(
      connectionRuleId: $connectionRuleId
      pageSize: $pageSize
      pageNo: $pageNo
    ) {
      id
      connectionRuleName
      logAction
      logType
      logResult
      message
      dateCreated
      connectionRuleID
    }
    logCount(connectionRuleId: $connectionRuleId)
  }
`;

const CONNECTION = gql`
  query getConnection($id: ID) {
    connections(id: $id) {
      id
      name
      destinationPlatformCfg
      sourcePlatformCfg
      enabled
      dateCreated
      connectionRules {
        id
        name
        enabled
        lastExecuted
        lastExecutedStatus
        syncIntervalCron
        fieldMappings
        connectionID
        sourcePostSyncTasks
        destinationPostSyncTasks
        jsonSourceContainerCfg {
          containerID
        }
        jsonDestinationContainerCfg {
          containerID
        }
      }
    }
    sourceContainers(connectionId: $id) {
      id
      name
    }
    destinationContainers(connectionId: $id) {
      id
      name
    }
  }
`;

const NEW_RULE = gql`
  mutation NewConnection($connectionId: Int!) {
    createConnectionRule(connectionId: $connectionId) {
      id
    }
  }
`;

const UPDATE_RULE = gql`
  mutation($connectionRuleInput: connectionRuleInput!) {
    updateRule(connectionRule: $connectionRuleInput) {
      name
    }
  }
`;

const DELETE_RULE = gql`
  mutation($connectionRuleId: Int!) {
    deleteConnectionRule(connectionRuleId: $connectionRuleId) {
      name
    }
  }
`;

function Rules(props) {
  const classes = useStyles();
  const [currentConnection, setCurrentConnection] = useState({});
  let history = useHistory();
  const user = window.userInfo;

  const [toastMesage, setToastMesage] = useState("");
  const [toastStatus, setToastStatus] = useState("info");
  const [openToast, setOpenToast] = React.useState(false);

  const [connectionRulesSyncing, setConnectionRulesSyncing] = useState([]);

  // const [statusMessage, setStatusMessage] = useState("");
  // const [statusTranscript, setStatusTranscript] = useState([]);

  const [openTranscript, setOpenTranscript] = useState(false);

  const logRef = useQuery(LOGS, {
    variables: {
      connectionRuleId: props.match.params.connectionRuleId,
      pageSize: 10,
      pageNo: 1
    },
    notifyOnNetworkStatusChange: true,
    //onCompleted:()=>setRowsPerPage(rowsPerPage)
  });



  const { loading, error, data,refetch } = useQuery(CONNECTION, {
    variables: { id: props.match.params.connectionId },
    onCompleted: () => {
      let cc = _.find(
        data.connections,
        o => o.id == props.match.params.connectionId
      );
      setCurrentConnection(cc);
    },
    notifyOnNetworkStatusChange: true
  });

  const [
    deleteRule,
    { deleteData, deleteLoading, deleteError, deleteCalled }
  ] = useMutation(DELETE_RULE, {
    refetchQueries: ["getConnection"],
    onCompleted: data => {
      setToastStatus("success");
      setToastMesage("The rule has been removed from the database");
      setOpenToast(true);
    }
  });

  const [
    updateRule,
    { updateData, updateLoading, updateError, updateCalled }
  ] = useMutation(UPDATE_RULE, {
    refetchQueries: ["getConnection"],
    onCompleted: data => {
      setToastStatus("success");
      setToastMesage("This rule has been updated");
      setOpenToast(true);
    }
  });

  const [newRule] = useMutation(NEW_RULE, {
    refetchQueries: ["getConnection"],
    variables: { connectionId: props.match.params.connectionId },
    onCompleted: data => {
      setToastStatus("success");
      setToastMesage(
        "New connection rule has been created. It must be configured before it can be used"
      );
      setOpenToast(true);
    }
  });



  const getSourceFolder = id => {
    let sourcefolder = _.find(data.sourceContainers, o => o.id == id);

    // console.log(id);
    // console.log(data.sourceContainers);
    // console.log(sourcefolder);

    if (sourcefolder) return sourcefolder;
    return { name: "This folder no longer exists" };
  };

  const getDestinationFolder = id => {
    let destinationfolder = _.find(data.destinationContainers, o => o.id == id);

    if (destinationfolder) return destinationfolder;
    return { name: "This folder no longer exists" };
  };

  // props.websocket.onopen = function(evt) {
  //   //console.log(evt);
  //   console.log(`socket open on rule ${props.id}`);
  // };

  props.websocket.onmessage = function(evt) {
    var obj = JSON.parse(evt.data);

    console.log(obj);

    //connectionRuleID
    if (obj.data && obj.data.connectionRuleID) 
    {
      //console.log(connectionRulesSyncing.indexOf(obj.data.connectionRuleID));
      if (obj.socketFrameType == 3) 
      {
        //console.log("should be removing sync")

        var newArray = _.remove(connectionRulesSyncing, function(n) 
        {
          //console.log(n, obj.data.connectionRuleID);
          return n != obj.data.connectionRuleID;
        });
        //console.log(newArray)

        setConnectionRulesSyncing(newArray);
        //connectionRulesSyncing.indexOf(obj.data.connectionRuleID)
      }
      else
      {
        if (connectionRulesSyncing.indexOf(obj.data.connectionRuleID) < 0) 
        {
          setConnectionRulesSyncing(connectionRulesSyncing.concat([obj.data.connectionRuleID]));
        }
      }
    }

    //obj.data.connectionRuleID
    //if (obj.data && obj.data.connectionRuleID == props.match.params.connectionId) {
    // /ssetConnectionRulesSyncing =
    // if (obj.data && obj.data.LogAction == 2 && obj.data.LogType == 1) {
    // console.log(obj.data);
    if (obj.data && obj.data.SourceDesc) {
      var statusMesage = document.getElementById("statusmessage");
      if (statusMesage) statusMesage.innerHTML = obj.data.SourceDesc;
    }

    if (obj.data && obj.data.DocStats && obj.data.DocStats.Current) {
      var el = document.getElementById("progress");
      if (el)
        el.style.width = Math.floor((obj.data.DocStats.Current / obj.data.DocStats.Total) * 100) + "%";
    } //setStatusMessage(obj.data);
    //   // if(obj.data && obj.data.SourceDesc)  setStatusTranscript(statusTranscript => statusTranscript.concat([obj.data.SourceDesc]));
    //   }
    //}
    //}
    if (obj.socketFrameType == 3) {
        console.log("Going to update UI Row State for Rule " + obj.data.connectionRuleID + "...");
        
        
        refetch();
        logRef.refetch();



        if (obj.data && obj.data.ExecutionStatus) {
            console.log(obj.data.ExecutionStatus + " [" + obj.data.connectionRuleID + "]");
            //var statusMesage = document.getElementById("statusmessage");
            //if (statusMesage) statusMesage.innerHTML = obj.data.ExecutionStatus;
        }
    }
    console.log(obj.socketFrameType);
  };

  // console.log(props);
  const ConnectionRules = ruleData => {
    if (ruleData.rules.length > 0) {
      //console.log(ruleData);
      return ruleData.rules.map((rule, index) => {
        return (
          <Rule
            key={`rule-${index}`}
            {...rule}
            sourceFolder={getSourceFolder}
            destinationFolder={getDestinationFolder}
            connectionId={rule.connectionID}
            websocket={props.websocket}
            updateRule={updateRule}
            deleteRule={deleteRule}
            connectionRulesSyncing={connectionRulesSyncing}
            setOpenTranscript={setOpenTranscript}
            webSocketState={props.webSocketState}

          />
        );
      });
    } else {
      return (
        <Grid item xs={12}>
          <Typography variant="body1" className={classes.norules}>
            No rules are configured for ths connection yet
          </Typography>
        </Grid>
      );
    }
  };

  const pageLabel = `Rules for connection: ${currentConnection.name}`;

  const test = () => {
    setToastStatus("error");
    setToastMesage(
      "New xx connection rule has been created. It must be configured before it can be used"
    );
    setOpenToast(true);
  };

  if (error) {
    // console.log(error)

    const title = "An error has occured";
    var msg = "";
    if (error.message.indexOf("Platform not configured") > 0) 
    {
      msg = "It looks like the connection hasn't been configured correctly. Please check the settings and try again";
    }

    return (
      <div>
        <Typography variant="h2">{title}</Typography>
        <Typography variant="subtitle2">{msg}</Typography>

        <pre>{error.message}</pre>
      </div>
    );
  }

  return (
    <Grid container item xs={12} alignItems={"center"}>
      <Grid container item xs={12}>
        <Breadcrumbs aria-label="breadcrumb">
          <Link color="inherit" href="/#/">
            Home
          </Link>
          <Link color="inherit" href="/#/connections">
            Connections
          </Link>
          <Typography color="textPrimary">{pageLabel}</Typography>
        </Breadcrumbs>
      </Grid>
      <Grid container item xs={12}>
        <Typography variant="h2" className={classes.pageTitle}>
          {currentConnection.name}
        </Typography>
      </Grid>
      <Grid container item xs={12}>
        <Paper className={classes.paper}>
          <Grid container item xs={12} className={classes.gridHeader}>
            <Grid item container xs={1}></Grid>
            <Grid item container xs={3}>
              <Typography variant={"subtitle2"}>Name</Typography>
            </Grid>
            <Grid item container xs={3} alignContent="center">
              <Typography variant={"subtitle2"}>Status</Typography>
            </Grid>
            <Grid item xs={5} container>
              <Grid item container xs={4} alignContent="center">
                <Typography variant={"subtitle2"}>Source</Typography>
              </Grid>
              <Grid item container xs={4} alignContent="center">
                <Typography variant={"subtitle2"}>Destination</Typography>
              </Grid>
              <Grid item container xs={4} alignContent="center">
                <Typography variant={"subtitle2"}>Schedule</Typography>
              </Grid>
            </Grid>
          </Grid>

          <Grid container item xs={12} alignItems={"center"}>
            {data &&
              data.connections.map((connection, index) => {
                return (
                  <ConnectionRules
                    key={`connection-${index}`}
                    rules={connection.connectionRules}
                  />
                );
              })}

            {loading && <Loader />}
          </Grid>
          <Grid container xs={12} item justify="center" alignItems="center">
          {user.IsConnectionAdmin && (
            <Fab
              color="primary"
              aria-label="add"
              size="small"
              className={classes.fab}
              onClick={newRule}
            >
              <AddIcon className={classes.icon} />
            </Fab>)}
          </Grid>
        </Paper>
      </Grid>
      <GlobalToast
        status={toastStatus}
        message={toastMesage}
        open={openToast}
        setOpenToast={setOpenToast}
      />

      <Modal
        aria-labelledby="simple-modal-title"
        aria-describedby="simple-modal-description"
        open={openTranscript}
        onClose={e => setOpenTranscript(false)}
      >
        <iframe src="/home/logviewer" className={classes.log}></iframe>
      </Modal>
    </Grid>
  );
}

export default Rules;