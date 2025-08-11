import gql from "graphql-tag";

export const PLATFORMS = gql`
query MyQuery {
  __typename
  platforms {
    name
    platformID
  }
}
`;

