import React from 'react';
import PropTypes from 'prop-types';
import moment from 'moment';
//import { Test } from './FormattedDate.styles';

const FormattedDate = (props) => {
 return  moment(new Date(props.date + 'Z')).format('MMMM Do YYYY, h:mm:ss a');
}




export default FormattedDate;
