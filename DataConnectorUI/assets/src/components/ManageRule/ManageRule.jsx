import React, { useState, useEffect } from "react";

import {
  Typography,
  TextField,
  Grid,
  Switch,
  FormControl,
  Select,
  InputLabel,
  MenuItem,
  Button,
  Paper,
  Breadcrumbs,
  Link,
  makeStyles,
  CircularProgress
} from "@material-ui/core";

import { CRON_PATTERNS } from "../../constants";


const useStyles = makeStyles(theme => ({
  paper: {
    width: "100%",
    marginTop: theme.spacing(4),
    position: "relative",
    padding:theme.spacing(4)
  },
  pageTitle:{
    marginTop:theme.spacing(4),
    marginBottom:theme.spacing(4),
  }
}));






function ManageRule(props) {

  const classes = useStyles();
  // const [sourceFolder,setSourceFolders] = useState({});
  // const [destinationFolder,setDestinationFolders] = useState({});


  
  
  const {
    rule,
    sourceContainers,
    destinationContainers,
    updateHandler,
    setSourceContainerId,
    setDestinationContainerId
  } = props;

  // console.log(rule);


  useEffect(() => {
    // console.log(props.sourceContainers)
    
    //setSourceFolders(props.rule.)
  }, [props.sourceContainers]);


  // if (!_.isEmpty(rule))
  return (
    <Grid container item xs direction="column" spacing={1}>
      <Grid item xs={12} container spacing={1}>




        <Grid
          item
          xs={12}
          container
          direction="row"
          alignContent="center"
          alignItems="center"
          spacing={4}
        >
          <Grid item xs={10}>
            <TextField
              required
              key="RuleName"
              fullWidth
              name="name"
              onChange={updateHandler}
              id="name"
              label="Rule Name"
              value={rule.name}
              className={classes.textField}
              margin="normal"
            />
          </Grid>
          <Grid item xs={2} container alignContent="center" alignItems="center">
            <Grid component="label" container alignItems="center" spacing={1}>
              <Grid item>
                <Typography variant="body1">Off</Typography>
              </Grid>
              <Grid item>
                <Switch
                  checked={rule.enabled}
                  onChange={updateHandler}
                  value={rule.enabled}
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
        <Grid item xs={12} container direction="row" spacing={4}>
          <Grid item xs={4}>
            <FormControl className={classes.formControl} fullWidth>
              <InputLabel shrink htmlFor="source-label-placeholder">
                Source Folder
              </InputLabel>
              <Select
                value={(rule.jsonSourceContainerCfg && rule.jsonSourceContainerCfg.containerID)?rule.jsonSourceContainerCfg.containerID:{}}
                onChange={e => {
                  setSourceContainerId(e.target.value.id);
                  updateHandler(e);
                }}
                renderValue={selected => {
                  // console.log(selected);
                  if (_.isEmpty(selected)) {
                    return <em>Select a folder</em>;
                  }else{
                    // console.log(selected);
                    var container = _.find(sourceContainers, function(o) { 
                      return o.id == selected; 
                    })
                    if(container)  return container.name || container.containerID;
                    return "Container no longer exists";

                  }
                }}
                inputProps={{
                  name: "jsonSourceContainerCfg",
                  id: "jsonSourceContainerCfg"
                }}
                displayEmpty
                name="jsonSourceContainerCfg"
                className={classes.selectEmpty}
              >
                {sourceContainers && sourceContainers.map(o => {
                  return (
                    <MenuItem key={`srcfolder-${o.id}`} value={{containerID:o.id}}>
                      {o.name}
                    </MenuItem>
                  );
                })}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={4}>
            <FormControl className={classes.formControl} fullWidth>
              <InputLabel shrink htmlFor="destination-label-placeholder">
                Destination Folder
              </InputLabel>
              <Select
                value={(rule.jsonDestinationContainerCfg && rule.jsonDestinationContainerCfg.containerID)?rule.jsonDestinationContainerCfg.containerID:{}}
                onChange={e => {
                  setDestinationContainerId(e.target.value.id);
                  updateHandler(e);
                }}
                inputProps={{
                  name: "jsonDestinationContainerCfg",
                  id: "jsonDestinationContainerCfg"
                }}
                displayEmpty
                name="jsonDestinationContainerCfg"
                className={classes.selectEmpty}

                renderValue={selected => {
                
                  // console.log(selected);
                  if (_.isEmpty(selected)) {
                    return <em>Select a folder</em>;
                  }else{
                    // console.log(selected);
                    var container = _.find(destinationContainers, function(o) { 
                      return o.id == selected; 
                    })
                    if(container)  return container.name || container.containerID;
                    return "Container no longer exists";




                  }
                }}
              >
                {destinationContainers && destinationContainers.map(o => {
                  return (
                    <MenuItem key={`destfolder-${o.id}`} value={{containerID:o.id}}>
                      {o.name}
                    </MenuItem>
                  );
                })}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={4}>
            <FormControl className={classes.formControl} fullWidth>
              <InputLabel shrink htmlFor="cron-label-placeholder">
                Schedule
              </InputLabel>
              <Select
                value={rule.syncIntervalCron || ""}
                onChange={updateHandler}
                inputProps={{
                  name: "syncIntervalCron",
                  id: "cron-label-placeholder"
                }}
                displayEmpty
                name="syncIntervalCron"
                className={classes.selectEmpty}
              >
                {CRON_PATTERNS.map((cron, index) => (
                  <MenuItem key={index} value={cron.pattern}>
                    {cron.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Grid>
    </Grid>
  );
}

export default ManageRule;