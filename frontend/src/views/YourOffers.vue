<template>
  <v-container fluid>
    <v-slide-y-transition mode="out-in">
      <v-row>
        <v-col>
          <h1>Your Offers</h1>
          <p>Here you can view all your offers.</p>

          <v-data-table
            :headers="headers"
            :items="offers"
            hide-default-footer
            :loading="loading"
            class="elevation-1"
            disable-pagination
          >
            <v-progress-linear v-slot:progress color="blue" indeterminate></v-progress-linear>
            <template v-slot:item.date="{ item }">
              <td>{{ item.date | date }}</td>
            </template>
            <template v-slot:item.spotsAvailable="{ item }">
              <v-chip dark>{{ item.spotsAvailable }}</v-chip>
            </template>
          </v-data-table>
        </v-col>
      </v-row>
    </v-slide-y-transition>

    <v-alert :value="showError" type="error" v-text="errorMessage" />
  </v-container>
</template>

<script lang="ts">
// an example of a Vue Typescript component using Vue.extend
import Vue from 'vue';
import { Offer } from '../models/Offer';
import { Location } from '../models/Location';
import { Vehicle } from '../models/Vehicle';

import axios from 'axios';
import getConfig from '../auth';

export default Vue.extend({
  data() {
    return {
      loading: true,
      showError: false,
      errorMessage: 'Error while loading loading offers.',
      offers: [],
      headers: [
        { text: 'User', value: 'user.name' },
        { text: 'From', value: 'from.title' },
        { text: 'To', value: 'to.title' },
        { text: 'Arrival Time', value: 'arrivalTime' },
        { text: 'Vehicle', value: 'vehicle.brand' },
        { text: 'Remaining Capacity', value: 'remainingCapacity' },
      ],
    };
  },
  methods: {

    async fetchOffers() {
      try {
        const config = getConfig();
        const response = await axios.get('/offer', config);
        this.offers = response.data;
      } catch (e) {
        this.showError = true;
        this.errorMessage = `Error while loading rides: ${e.message}.`;
      }
      this.loading = false;
    },
  },
  async created() {
    await this.fetchOffers();
  },
});
</script>
