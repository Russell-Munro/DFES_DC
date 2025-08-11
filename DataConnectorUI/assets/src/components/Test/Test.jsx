import React, { PureComponent } from 'react';
import PropTypes from 'prop-types';
import { Typography, withStyles, Container } from '@material-ui/core';
import { withTheme, ThemeProvider } from '@material-ui/styles';
//import { Test } from './Test.styles';

const styles = theme => ({
  root: {
    marginTop: theme.spacing(13),
    width: '100%'
  },
  flex: {
    flex: 1
  },
  menuButton: {
    marginLeft: -12,
    marginRight: 20
  }
})

class Test extends PureComponent { 
  constructor(props) {
    super(props);
  }

  render () {
    const {classes} = this.props;

    return (
      <Container maxWidth="sm" className={classes.root}>
        <Typography >hello</Typography>
      </Container>
    );
  }
}

Test.propTypes = {
  classes: PropTypes.object.isRequired
};

Test.defaultProps = {
  // bla: 'test',
};

export default withStyles(styles)(Test);
