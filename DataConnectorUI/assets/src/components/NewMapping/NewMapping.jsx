import React, { useState } from "react";
import _ from "lodash";

//components
import {
  Grid,
  Typography,
  Select,
  MenuItem,
  IconButton,
  Paper,
  makeStyles,
  Fade,
  FormControlLabel,
  Checkbox
} from "@material-ui/core";

import TrendingFlatIcon from "@material-ui/icons/TrendingFlat";

import AddIcon from "@material-ui/icons/Add";
import Loader from "../Loader/Loader";

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
  title: {
    marginTop: theme.spacing(10),
    marginBottom: theme.spacing(4)
  },
  select: {
    borderRadius: theme.shape.borderRadius,
    padding: theme.spacing(4),
    borderRadius: theme.shape.borderRadius
  }
}));

function NewMapping(props) {
  // console.log(props.connectionId, props.sourceContainerId);
  const classes = useStyles();

  const [sourceField, setSourceField] = useState({});
  const [options, setOptions] = useState({
    alwaysUpdateFromSrc: false,
    mutuallyExclusive: false,
    nullAction: 0
  });
  const [destinationField, setDestinationField] = useState({});
  const [isValid, setIsValid] = useState(false);
  const { sourceFields, destinationFields } = props;

  const calculateIsValid = () => {
    console.log(sourceField);
    setIsValid(!(_.isEmpty(sourceField) && _.isEmpty(destinationField)));
  };

  const updateOptions = o => {
    console.log(o);
    setOptions(Object.assign({}, options, o));
  };

  const handleNewMapping = () => {
    props.addMapping({ srcField: sourceField, destField: destinationField, options: options });

    setSourceField({});
    setDestinationField({});
    setOptions({
      alwaysUpdateFromSrc: false,
      mutuallyExclusive: false,
      nullAction: 1
    });

    setIsValid(false);
  };

  // console.log(sourceFields, destinationFields);

  if (sourceFields != null && destinationFields != null) {
    return (
      <Fade in={true}>
        <Grid container item xs={12}>
          <Grid item xs={12}>
            <Typography variant={"h5"} className={classes.title}>
              Add a New Mapping
            </Typography>

          </Grid>

          <Grid item xs={6} container>
            <Grid item xs={5}><Typography variant="subtitle2">Source Field</Typography></Grid>
            <Grid item xs={2}></Grid>
            <Grid item xs={5}><Typography variant="subtitle2">Destination Field</Typography></Grid>
          </Grid>

          <Grid item xs={6} container>
            <Grid item xs={3} container justify="center" alignItems="center"><Typography variant="subtitle2">Always Update</Typography></Grid>
            <Grid item xs={3} container justify="center" alignItems="center"><Typography variant="subtitle2">Exclusive</Typography></Grid>
            <Grid item xs={3}><Typography variant="subtitle2">When No Value</Typography></Grid>
            <Grid item xs={3}></Grid>
          </Grid>

          <Grid item xs={12} container>

          <Grid item xs={6} container>
            <Grid item xs={5}>
              
              <Select
                fullWidth
                value={sourceField}
                classes={{ select: classes.select }}
                renderValue={selected => {
                  // console.log(selected);
                  if (_.isEmpty(selected)) {
                    return <em>Select a field</em>;
                  }
                  return `${sourceField.title} - (${sourceField.key})`;
                }}
                onChange={e => {
                  setSourceField(e.target.value);
                  calculateIsValid();
                }}
                inputProps={{
                  name: `new-source`,
                  id: `new-source`
                }}
              >
                <MenuItem disabled value="">
                  <em>Select a field</em>
                </MenuItem>

                {sourceFields.map((field, fieldIndex) => (
                  <MenuItem key={`new-source-${fieldIndex}`} value={field}>
                    {field.title} - ({field.key})
                  </MenuItem>
                ))}
              </Select>
            </Grid>

            <Grid item xs={2} container justify="center" alignItems="center">
              <TrendingFlatIcon />
            </Grid>

            <Grid item xs={5}>
              <Select
                fullWidth
                value={destinationField}
                classes={{ select: classes.select }}
                renderValue={selected => {
                  // console.log(selected);
                  if (_.isEmpty(selected)) {
                    return <em>Select a field</em>;
                  }
                  return destinationField.title;
                }}
                onChange={e => {
                  setDestinationField(e.target.value);
                  calculateIsValid();
                }}
                inputProps={{
                  name: `new-destination`,
                  id: `new-destination`
                }}
              >
                <MenuItem disabled value="">
                  <em>Select a field</em>
                </MenuItem>

                {/* {console.log(destinationFields)} */}
                {destinationFields.map((field, fieldIndex) => (
                  <MenuItem key={`new-destination-${fieldIndex}`} value={field}>
                    {field.title} - ({field.key})
                  </MenuItem>
                ))}
              </Select>
              </Grid>
            </Grid>

            <Grid container item xs={6}>
              <Grid  container justify="center" alignItems="center" item xs={3}>
                <Checkbox
                  checked={options.alwaysUpdateFromSrc}
                  onChange={e =>
                    updateOptions({
                      alwaysUpdateFromSrc: !options.alwaysUpdateFromSrc
                    })
                  }
                  value={true}
                  color="primary"
                />
              </Grid>

              <Grid  item xs={3} container justify="center" alignItems="center">
                <Checkbox
                  checked={options.mutuallyExclusive}
                  onChange={e =>
                    updateOptions({
                      mutuallyExclusive: !options.mutuallyExclusive
                    })
                  }
                  value={true}
                  color="primary"
                />
              </Grid>

              <Grid container item xs={3}>
                <Select
                  fullWidth
                  value={options.nullAction}
                  classes={{ select: classes.select }}
                  // renderValue={selected => {
                  //   // console.log(selected);
                  //   if (_.isEmpty(selected)) {
                  //     return <em>Select a field</em>;
                  //   }
                  //   return destinationField.title;
                  // }}
                  onChange={e => {

                    console.log(e.target.value)

                    updateOptions({
                      nullAction: e.target.value,
                      nullActionLabel:props.nullActions[e.target.value-1]
                    });
                    calculateIsValid();
                  }}
                  inputProps={{
                    name: `nullAction`,
                    id: `nullAction`
                  }}
                >
                  <MenuItem disabled value="">
                    <em>Select a field</em>
                  </MenuItem>

                  {/* {console.log(destinationFields)} */}
                  {props.nullActions.map((action, idx) => (
                    <MenuItem key={`naction-${idx}`} value={idx + 1}>
                      {action}
                    </MenuItem>
                  ))}
                </Select>
              </Grid>
              <Grid item xs={3} container justify="center" alignItems="center">
              <IconButton
                color="secondary"
                aria-label="add"
                disabled={!isValid}
                onClick={handleNewMapping}
              >
                <AddIcon />
              </IconButton>
            </Grid>
            </Grid>
           
          </Grid>
        </Grid>
      </Fade>
    );
  }

  return <Loader />;
}

export default NewMapping;
