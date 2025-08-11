import React from 'react';

//components
import { Grid, Typography, Paper, Fade, Checkbox } from '@material-ui/core';
import DeleteOutlineIcon from '@material-ui/icons/DeleteOutline';
//constants
import TrendingFlatIcon from '@material-ui/icons/TrendingFlat';
import { makeStyles } from '@material-ui/styles';

const useStyles = makeStyles(theme => ({
  paper: {
    width: '100%',
    marginTop: theme.spacing(4),
    position: 'relative',
    padding: theme.spacing(4),
  },
  title: {
    marginTop: theme.spacing(10),
    marginBottom: theme.spacing(4),
  },
  fab: {
    margin: theme.spacing(1),
    position: 'fixed',
    bottom: theme.spacing(2),
    right: theme.spacing(2),
  },
  pending: {
    margin: theme.spacing(10),
  },
  mappingField: {
    backgroundColor: theme.palette.grey['200'],
    padding: theme.spacing(4),
    marginTop: theme.spacing(1),
    marginBottom: theme.spacing(1),
    borderRadius: theme.shape.borderRadius,
  },
}));

function MappingsBuilder(props) {
  const classes = useStyles();
  const { rule, deleteAction } = props;

  if (rule.jsonFieldMappings.length > 0) {
    return (
      <Fade in={true}>
        <Grid container itemxs={12}>
          <Grid item xs={12}>
            <Typography variant="h5" className={classes.title}>
              Field Mappings
            </Typography>
          </Grid>
          <Grid item xs={12} container>
            <Grid item xs={6} container>
              <Grid item xs={5}>
                <Typography variant="subtitle2">Source Field</Typography>
              </Grid>
              <Grid item xs={2}></Grid>
              <Grid item xs={5}>
                <Typography variant="subtitle2">Destination Field</Typography>
              </Grid>
            </Grid>

            <Grid item xs={6} container>
              <Grid item xs={3} container justify="center" alignItems="center">
                <Typography variant="subtitle2">Always Update</Typography>
              </Grid>
              <Grid item xs={3} container justify="center" alignItems="center">
                <Typography variant="subtitle2">Exclusive</Typography>
              </Grid>
              <Grid item xs={3}>
                <Typography variant="subtitle2">When No Value</Typography>
              </Grid>
              <Grid item xs={3}></Grid>
            </Grid>
          </Grid>
          {rule &&
            rule.jsonFieldMappings.length > 0 &&
            rule.jsonFieldMappings.map((o, index) => {
              //console.log(o);
              return (
                <Grid key={`map-${index}`} container spacer={10} item xs={12}>
                  <Grid item xs={6} container>
                    <Grid item xs={5}>
                      
                      <Typography
                        variant={'body1'}
                        className={classes.mappingField}
                      >
                        {`${o.srcField.title} - ${o.srcField.key}`}
                      </Typography>
                    </Grid>
                    <Grid
                      item
                      xs={2}
                      container
                      justify="center"
                      alignItems="center"
                    >
                      <TrendingFlatIcon />
                    </Grid>
                    <Grid item xs={5}>
                      
                      <Typography
                        variant={'body1'}
                        className={classes.mappingField}
                      >
                        {`${o.destField.title} - ${o.destField.key}`}
                      </Typography>
                    </Grid>
                  </Grid>

                  <Grid item xs={6} container>
                    <Grid
                      item
                      xs={3}
                      container
                      justify="center"
                      alignItems="center"
                    >
                      <Checkbox
                        checked={o.options.alwaysUpdateFromSrc}
                        color="default"
                        readOnly={true}
                      />
                    </Grid>
                    <Grid
                      item
                      xs={3}
                      container
                      justify="center"
                      alignItems="center"
                    >
                      <Checkbox
                        checked={o.options.mutuallyExclusive}
                        color="default"
                        readOnly={true}
                      />
                    </Grid>
                    <Grid item xs={3} container justify="flex-start" alignItems="center"><Typography variant="body1">{o.options.nullActionLabel}</Typography></Grid>
                    <Grid item xs={3} container justify="center" alignItems="center">
                      
                      <DeleteOutlineIcon
                        onClick={e => deleteAction(e, index)}
                      />
                    </Grid>
                  </Grid>
                </Grid>
              );
            })}
        </Grid>
      </Fade>
    );
  }
  return '';
}

export default MappingsBuilder;
