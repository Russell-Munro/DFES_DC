import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import {
  Grid,
  Typography,
  TextField,
  Button,
  Fade,
  Slide,
  Breadcrumbs,
  Link,
  IconButton,
  Paper,
  Select,
  MenuItem,
  InputLabel,
  FormControl,
  InputAdornment,
  Input,
  Switch
} from "@material-ui/core";
import { useHistory } from "react-router-dom";

import TrendingFlatIcon from "@material-ui/icons/TrendingFlat";
import Loader from "../../../components/Loader/Loader";

import Visibility from "@material-ui/icons/Visibility";
import VisibilityOff from "@material-ui/icons/VisibilityOff";
//import { Test } from './NewConnection.styles';
import gql from "graphql-tag";
import { useQuery, useMutation } from "@apollo/react-hooks";
import { makeStyles } from "@material-ui/styles";
import toPascal from "../../../lib/toPascal";
// import { PLATFORMS } from "../../../Data/Queries";

// const ADD_CONNECTION = gql`
//   mutation($newConnection: connectionInput!) {
//     createConnection(newConnection: $newConnection) {
//       name
//       id
//       enabled
//       __typename
//     }
//   }
// `;

const UPDATE_CONNECTION = gql`
  mutation($connection: connectionInput!) {
    updateConnection(connection: $connection) {
      id
      name
      enabled
      destinationPlatformCfg
      sourcePlatformCfg
      jsonSourcePlatformCfg {
        endPointURL
        integratorID
        servicePassword
        serviceUsername
        serviceDomain
        platformID

      }
      jsonDestinationPlatformCfg {
        endPointURL
        integratorID
        servicePassword
        serviceUsername
        serviceDomain
        platformID

      }
    }
  }
`;

const CONNECTIONS = gql`
  query getConnections {
    connections {
      id
      name
      enabled
      destinationPlatformCfg
      sourcePlatformCfg
      jsonSourcePlatformCfg {
        endPointURL
        integratorID
        servicePassword
        serviceUsername
        serviceDomain
        platformID

      }
      jsonDestinationPlatformCfg {
        endPointURL
        integratorID
        servicePassword
        serviceUsername
        serviceDomain
        platformID

      }
    }
  }
`;

const PLATFORMS = gql`
  query getSourceIntegrators($srcPlatformID: ID, $destPlatformID: ID) {
    sourceIntegrators(platformID: $srcPlatformID) {
      integratorID
      name
    }
    destinationIntegrators(platformID: $destPlatformID) {
      integratorID
      name
    }
    platforms {
      name
      platformID
    }
    integrators {
      name
      integratorID
      platformConfig
    }
  }
`;

const useStyles = makeStyles(theme => ({
  paper: {
    width: "100%",
    marginTop: theme.spacing(4),
    position: "relative",
    padding: theme.spacing(4)
  },
  pageTitle: {
    marginTop: theme.spacing(4),
    marginBottom: theme.spacing(4)
  },
  actionBar: { marginTop: theme.spacing(4), marginBottom: theme.spacing(4) },
  select: {
    borderRadius: theme.shape.borderRadius,
    padding: theme.spacing(4),
    borderRadius: theme.shape.borderRadius
  },
  formControl: {
    marginTop: theme.spacing(4)
  },
  platform: {
    backgroundColor: theme.palette.grey["200"],
    padding: theme.spacing(4),
    marginTop: theme.spacing(1),
    marginBottom: theme.spacing(1),
    borderRadius: theme.shape.borderRadius
  },
  savebutton: {
    color: "white"
  }
}));

function ConnectionForm(props) {
  const user = window.userInfo;
  if (!user.IsConnectionAdmin) return "You shouldnt be here"

  const classes = useStyles();
  let history = useHistory();

  const [name, setName] = useState("New Connection");
  const [id, setId] = useState();
  const [enabled, setEnabled] = useState(false);

  const [platforms, setPlatforms] = useState([]);
  // const [recordExists, setRecordExists] = useState(false);
  // console.log(props)

  
  const [sourcePlatform, setSourcePlatform] = useState({});
  const [sourcePlatformIntegrator, setSourcePlatformIntegrator] = useState({});
  const [sourcePlatformCfg, setSourcePlatformCfg] = useState({
    endPointURL: "",
    integratorID: "",
    servicePassword: "",
    serviceUsername: "",
    serviceDomain: "",
    additionalConfigs: "",
    __typename: "PlatformCfgType"
  });
  const [showSourcePassword, setShowSourcePassword] = useState(false);

  const [destinationPlatform, setDestinationPlatform] = useState({});
  const [
    destinationPlatformIntegrator,
    setDestinationPlatformIntegrator
  ] = useState({});
  const [destinationPlatformCfg, setDestinationPlatformCfg] = useState({
    endPointURL: "",
    integratorID: "",
    servicePassword: "",
    serviceUsername: "",
    serviceDomain: "",
    additionalConfigs: "",
    __typename: "PlatformCfgType"
  });
  const [showDestinationPassword, setShowDestinationPassword] = useState(false);

    // helper (top-level in component)
    const parseCfg = (s) => {
        try { return JSON.parse(s || "{}"); } catch { return {}; }
    };

  /**
   * QUERIES
   */
    const currentConnectionQuery = useQuery(CONNECTIONS, {
        variables: { id: props.match.params.connectionId },
        onCompleted: data => {
            const cc = _.find(data.connections, o => o.id == props.match.params.connectionId);
            if (!cc) return;

            setName(cc.name);
            setId(cc.id);
            setEnabled(cc.enabled);

            const srcRaw = parseCfg(cc.sourcePlatformCfg);
            const destRaw = parseCfg(cc.destinationPlatformCfg);

            // seed from typed JSON, then patch in AdditionalConfigs from raw string
            setSourcePlatformCfg(prev => ({
                ...prev,
                ...cc.jsonSourcePlatformCfg,
                additionalConfigs: srcRaw.AdditionalConfigs || srcRaw.additionalConfigs || ""
            }));

            setDestinationPlatformCfg(prev => ({
                ...prev,
                ...cc.jsonDestinationPlatformCfg,
                additionalConfigs: destRaw.AdditionalConfigs || destRaw.additionalConfigs || ""
            }));
        }
    });

  console.log(currentConnectionQuery)

  const platformQuery = useQuery(PLATFORMS, {
    variables: {
      srcPlatformID: sourcePlatform.platformID,
      destPlatformID: destinationPlatform.platformID
    },
    onCompleted: data => {
      setPlatforms(data.platforms);
    }
  });


  /**
   * MUTATIONS
   */

  const [updateConnection, { updatedata }] = useMutation(UPDATE_CONNECTION, {
    refetchQueries:['getConnections'],
    update(cache, { data }) {

      console.log(data); 
      
      const cacheValue = cache.readQuery({ query: CONNECTIONS });
      // console.log(data.createConnection);
      cache.writeQuery({
        query: CONNECTIONS,
        data: { connections: cacheValue.connections.push(data.updateConnection) }
      });

      history.push("/connections");
    }
  });


  useEffect(() => {

    console.log("srcp called");
    console.log(platformQuery.data);

    if (platformQuery.data && platformQuery.data.integrators) {
      //have the list of integrators

      if (sourcePlatformCfg.integratorID != "") {

        var sourceIntegrator = _.find(
          platformQuery.data.integrators,
          o => o.integratorID == sourcePlatformCfg.integratorID
        );

        console.log(sourceIntegrator);
        setSourcePlatformIntegrator(sourceIntegrator);
      }

      //set destination config
      if (destinationPlatformCfg.integratorID != "") {
        var destinationIntegrator = _.find(
          platformQuery.data.integrators,
          o => o.integratorID == destinationPlatformCfg.integratorID
        );

        setDestinationPlatformIntegrator(destinationIntegrator);
      }

      console.log(sourcePlatformCfg);
      if (sourcePlatformCfg.platformID != "") {
        var sourcePlt = _.find(
          platformQuery.data.platforms,
          o => o.platformID == sourcePlatformCfg.platformID
        );

        console.log(sourcePlt);
        if (sourcePlt != null) setSourcePlatform(sourcePlt);
      }

      if (destinationPlatformCfg.platformID != "") {
        var destPlt = _.find(
          platformQuery.data.platforms,
          o => o.platformID == destinationPlatformCfg.platformID
        );

        if (destPlt != null) setDestinationPlatform(destPlt);
      }
    }
  }, [sourcePlatformCfg, destinationPlatformCfg, platforms]);

//  
const StringifyConfig = o => {
  return JSON.stringify(toPascal(o));
}

  const saveConnection = () => {
    // if (!recordExists) {
    //   createConnection({
    //     variables: {
    //       newConnection: {
    //         name: name,
    //         sourcePlatformCfg: JSON.stringify(sourcePlatformCfg),
    //         destinationPlatformCfg: JSON.stringify(destinationPlatformCfg),
    //         enabled:enabled
    //       }
    //     }
    //   });
    // } else {

    StringifyConfig(sourcePlatformCfg);


    updateConnection({
        variables: {
          connection: {
            name: name,
            sourcePlatformCfg: StringifyConfig(sourcePlatformCfg),
            destinationPlatformCfg: StringifyConfig(destinationPlatformCfg),
            id: id,
            enabled:enabled

          }
        }
      });
    // }
  };

  const handleSourceIntegratorChange = e => {
    var val = e.target.value;
    setSourcePlatformIntegrator(val);
    var newSource = Object.assign({}, sourcePlatformCfg, {
      integratorID: val.integratorID
    });
    setSourcePlatformCfg(newSource);
  };

  const handleSrcConfigChange = e => {
    // console.log(e.target.id);
    // console.log(e.target.value);

    var newObject = {};
    newObject[e.target.name] = e.target.value;
    var newSource = Object.assign({}, sourcePlatformCfg, newObject);
    setSourcePlatformCfg(newSource);
  };

  const handleDestinationIntegratorChange = e => {
    var val = e.target.value;

    console.log(val);

    setDestinationPlatformIntegrator(val);
    var newSource = Object.assign({}, destinationPlatformCfg, {
      integratorID: val.integratorID
    });
    setDestinationPlatformCfg(newSource);
  };

  const handleSourcePlatformChange = e => {
      console.log(e.target.value);
    var newObject = {};
    newObject["platformID"] = e.target.value.platformID;
    var newSource = Object.assign({}, sourcePlatformCfg, newObject);
    setSourcePlatformCfg(newSource);
    setSourcePlatform(e.target.value);
  };

  const handleDestinationPlatformChange = e => {
    console.log(e.target.value);
    var newObject = {};
    newObject["platformID"] = e.target.value.platformID;
    var newSource = Object.assign({}, destinationPlatformCfg, newObject);
    setDestinationPlatformCfg(newSource);
    setDestinationPlatform(e.target.value);
  };

  const handleDestinationConfigChange = e => {
    console.log(e.target.name);
    console.log(e.target.value);

    var newObject = {};
    newObject[e.target.name] = e.target.value;
    var newSource = Object.assign({}, destinationPlatformCfg, newObject);
    setDestinationPlatformCfg(newSource);
  };

  const handleClickShowPassword = () => {
    setShowSourcePassword({ ...values, showPassword: !values.showPassword });
  };

  const handleMouseDownPassword = event => {
    event.preventDefault();
  };

  if(platformQuery.data){
  return (
    <Slide direction="left" in={true}>
      <Grid container item xs={12}>
        <Grid container item xs={12}>
          <Breadcrumbs aria-label="breadcrumb">
            <Link color="inherit" href="/#/connections">
              Connections
            </Link>
            <Typography color="textPrimary">
              {`Edit Connection Details`}
           </Typography>
          </Breadcrumbs>
        </Grid>
        <Grid item xs={12}>
          <Typography variant="h2">
            { `Edit Connection Details`}
          </Typography>
        </Grid>

        <Paper className={classes.paper}>
          <Grid item xs={12} container>
            <Grid
              item
              xs={10}
              container
              alignContent="center"
              alignItems="center"
            >
              <Input
                id="Name"
                label="Connection Name"
                fullWidth
                value={name}
                onChange={e => setName(e.target.value)}
              />
            </Grid>

            <Grid
              item
              xs={2}
              container
              alignContent="center"
              alignItems="center"
            >
              <Grid component="label" container alignItems="center" spacing={1}>
                <Grid item>
                  <Typography variant="body1">Off</Typography>
                </Grid>
                <Grid item>
                  <Switch
                    checked={enabled}
                    onChange={(e) => setEnabled(!enabled)}
                    value={enabled}
                    id="enabled"
                    name="enabled"
                    color="primary"
                    inputProps={{ "aria-label": "primary checkbox" }}
                  />
                </Grid>
                <Grid item>
                  <Typography variant="body1">On</Typography>
                </Grid>
              </Grid>
            </Grid>
          </Grid>
          <Grid container item xs={12}>
            <Grid item xs={12}>
              <Typography variant={"h6"} className={classes.formControl}>
                Platform Configuration
              </Typography>
            </Grid>
            <Grid item xs={5} className={classes.platform}>
              <Typography className={classes.formControl} variant={"subtitle2"}>
                Source Platform
              </Typography>

              <FormControl className={classes.formControl} fullWidth>
                {/* <InputLabel shrink htmlFor="age-native-label-placeholder">
                  Source Platform
                </InputLabel> */}
                <Select
                  fullWidth
                  value={sourcePlatform || {}}
                  onChange={(handleSourcePlatformChange)}
                  renderValue={selected => {
                    return sourcePlatform.name || sourcePlatform.id;
                  }}
                  inputProps={{
                    name: `sourcePlatformCfg`,
                    id: `sourcePlatformCfg`
                  }}
                >
                  {platforms.map((platform, index) => (
                    <MenuItem key={`platform-${index}`} value={platform}>
                      {platform.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>


              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="sourcePlatformIntegrator">
                  Select an Integrator
                </InputLabel>
                <Select
                  fullWidth
                  value={sourcePlatformIntegrator || {}}
                  onChange={handleSourceIntegratorChange}
                  renderValue={selected => {
                    if(sourcePlatformIntegrator){
                      return sourcePlatformIntegrator.name;
                    }
                    return "";
                  }}
                  inputProps={{
                    name: `sourcePlatformIntegrator`,
                    id: `sourcePlatformIntegrator`
                  }}
                >
                  {platformQuery.data &&
                    platformQuery.data.sourceIntegrators &&
                    platformQuery.data.sourceIntegrators.map(
                      (integrator, index) => (
                        <MenuItem
                          key={`integrator-${index}`}
                          value={integrator}
                        >
                          {integrator.name}
                        </MenuItem>
                      )
                    )}
                </Select>
              </FormControl>
              

              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="SourceConnectionUrl">
                  Connection Url
                </InputLabel>
                <Input
                  id="SourceEndPointURL"
                  name="endPointURL"
                  label="Connection Url"
                  fullWidth
                  value={sourcePlatformCfg.endPointURL || ""}
                  onChange={handleSrcConfigChange}
                />
              </FormControl>

              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="SourceServiceDomain">
                  Domain
                </InputLabel>
                <Input
                  id="SourceServiceDomain"
                  name="serviceDomain"
                  label="Domain"
                  fullWidth
                  value={sourcePlatformCfg.serviceDomain || ""}
                  onChange={handleSrcConfigChange}
                />
              </FormControl>

              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="SourceServiceUsername">
                  User Name
                </InputLabel>
                <Input
                  id="SourceServiceUsername"
                  name="serviceUsername"
                  label="User Name (clientId)"
                  fullWidth
                  value={sourcePlatformCfg.serviceUsername || ""}
                  onChange={handleSrcConfigChange}
                />
              </FormControl>

              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="SourceServicePassword">
                  Password
                </InputLabel>
                <Input
                  type={showSourcePassword ? "text" : "password"}
                  className={classes.formField}
                  id="SourceServicePassword"
                  name="servicePassword"
                  label="Password (ClientSecret)"
                  fullWidth
                  value={sourcePlatformCfg.servicePassword || ""}
                  onChange={handleSrcConfigChange}
                  endAdornment={
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="toggle password visibility"
                        onClick={() =>
                          setShowSourcePassword(!showSourcePassword)
                        }
                        onMouseDown={handleMouseDownPassword}
                      >
                        {showDestinationPassword ? (
                          <Visibility />
                        ) : (
                          <VisibilityOff />
                        )}
                      </IconButton>
                    </InputAdornment>
                  }
                />
              </FormControl>

              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="SourceAdditionalConfigs">
                  Additional Configs
                </InputLabel>
                <Input
                  id="SourceAdditionalConfigs"
                  name="additionalConfigs"
                  label="Additional Configs"
                  placeholder={'sitePath:"/sites/shared-resources";driveName:"EquDevCirculars";'}
                  fullWidth
                  value={sourcePlatformCfg.additionalConfigs || ""}
                  onChange={handleSrcConfigChange}
                />
              </FormControl>

              {/* <pre>{JSON.stringify(sourcePlatformCfg, null, 1)}</pre> */}
            </Grid>

            {/*


              DESTINATION


            */}

            <Grid item xs={2} container justify="center" alignItems="center">
              <TrendingFlatIcon />
            </Grid>

            <Grid item xs={5} className={classes.platform}>
              <Typography variant={"subtitle2"} className={classes.formControl}>
                Destination Platform
              </Typography>
              <FormControl className={classes.formControl} fullWidth>
                <Select
                  fullWidth
                  label="Source Platform"
                  value={destinationPlatform || {}}
                  onChange={handleDestinationPlatformChange}
                  renderValue={selected => {
                    return destinationPlatform.name;
                  }}
                  inputProps={{
                    name: `destinationPlatform`,
                    id: `destinationPlatform`
                  }}
                >
                  {platforms.map((platform, index) => (
                    <MenuItem key={`destplatform-${index}`} value={platform}>
                      {platform.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="destinationPlatformIntegrator">
                  Select an Integrator
                </InputLabel>
                <Select
                  fullWidth
                  value={destinationPlatformIntegrator || {}}
                  onChange={handleDestinationIntegratorChange}
                  renderValue={selected => {
                    if(destinationPlatformIntegrator){
                      return destinationPlatformIntegrator.name;

                    }
                    return "";
                  }}
                  inputProps={{
                    name: `destinationPlatformIntegrator`,
                    id: `destinationPlatformIntegrator`
                  }}
                >
                  {platformQuery.data &&
                    platformQuery.data.destinationIntegrators &&
                    platformQuery.data.destinationIntegrators.map(
                      (integrator, index) => (
                        <MenuItem
                          key={`destintegrator-${index}`}
                          value={integrator}
                        >
                          {integrator.name}
                        </MenuItem>
                      )
                    )}
                </Select>
              </FormControl>
              
              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="DestinationConnectionUrl">
                  Connection Url
                </InputLabel>
                <Input
                  id="DestinationConnectionUrl"
                  name="endPointURL"
                  label="Connection Url"
                  fullWidth
                  value={destinationPlatformCfg.endPointURL || ""}
                  onChange={handleDestinationConfigChange}
                />
              </FormControl>
              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="DestinationServiceDomain">
                  Domain
                </InputLabel>
                <Input
                  id="DestinationServiceDomain"
                  label="Domain"
                  name="serviceDomain"
                  fullWidth
                  value={destinationPlatformCfg.serviceDomain || ""}
                  onChange={handleDestinationConfigChange}
                />
              </FormControl>
              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="DestinationServiceUsername">
                  User Name
                </InputLabel>
                <Input
                  id="DestinationServiceUsername"
                  label="User Name"
                  name="serviceUsername"
                  fullWidth
                  value={destinationPlatformCfg.serviceUsername || ""}
                  onChange={handleDestinationConfigChange}
                />
              </FormControl>

              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="DestinationServicePassword">
                  Password
                </InputLabel>
                <Input
                  id="DestinationServicePassword"
                  type={showDestinationPassword ? "text" : "password"}
                  label="Password"
                  name="servicePassword"
                  fullWidth
                  value={destinationPlatformCfg.servicePassword || ""}
                  onChange={handleDestinationConfigChange}
                  endAdornment={
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="toggle password visibility"
                        onClick={() =>
                          setShowDestinationPassword(!showDestinationPassword)
                        }
                        onMouseDown={handleMouseDownPassword}
                      >
                        {showDestinationPassword ? (
                          <Visibility />
                        ) : (
                          <VisibilityOff />
                        )}
                      </IconButton>
                    </InputAdornment>
                  }
                />
              </FormControl>
              <FormControl className={classes.formControl} fullWidth>
                <InputLabel shrink htmlFor="DestinationAdditionalConfigs">
                  Additional Configs
                </InputLabel>
                <Input
                  id="DestinationAdditionalConfigs"
                  name="additionalConfigs"
                  label="Additional Configs"
                  placeholder={'sitePath:"/sites/shared-resources";driveName:"EquDevCirculars";'}
                  fullWidth
                  value={destinationPlatformCfg.additionalConfigs || ""}
                  onChange={handleDestinationConfigChange}
                />
              </FormControl>
              {/* {JSON.stringify(destinationPlatform, null, 1)} */}
            </Grid>
          </Grid>

          <Grid container item xs={12}>
            <Grid item xs={5}></Grid>

            <Grid item xs={2}></Grid>
            <Grid item xs={5}></Grid>
          </Grid>

          <Grid item xs={12} className={classes.formControl}>
            <Button
              variant="contained"
              color="primary"
              onClick={saveConnection}
              className={classes.savebutton}
            >
              {`Update Connection`}
            </Button>
          </Grid>
        </Paper>
      </Grid>
    </Slide>
  );
                }

return <Loader />

}

export default ConnectionForm;
