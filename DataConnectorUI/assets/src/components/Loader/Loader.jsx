import React from "react";
import PropTypes from "prop-types";
import { Grid, CircularProgress, makeStyles, Fade } from "@material-ui/core";
//import { Test } from './Loader.styles';

const useStyles = makeStyles(theme => ({
  loader: {
    minHeight: 100
  }
}));

function Loader(props) {
  const classes = useStyles();

  return (
    <Fade in={true}>
    <Grid container item xs={12} container justify="center" alignItems="center" className={classes.loader}>
      <CircularProgress color="secondary" />
    </Grid>
    </Fade>
  );
}

export default Loader;
