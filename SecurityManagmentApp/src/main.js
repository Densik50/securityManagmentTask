import { createApp } from 'vue'
import PrimeVue from 'primevue/config';
import App from './App.vue'
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import Checkbox from 'primevue/checkbox';
import Dropdown from 'primevue/dropdown';
import Tooltip from 'primevue/tooltip';

import 'primevue/resources/themes/mdc-dark-indigo/theme.css'
import 'primevue/resources/primevue.min.css';
import 'primeicons/primeicons.css';

const app = createApp(App);
app.use(PrimeVue);
app.directive('tooltip', Tooltip);
app.component('PrimeVueDataTable', DataTable);
app.component('PrimeVueColumn', Column);
app.component('PrimeVueDialog', Dialog);
app.component('PrimeVueButton', Button);
app.component('PrimeVueInputText', InputText);
app.component('PrimeVueInputNumber', InputNumber);
app.component('PrimeVueCheckbox', Checkbox);
app.component('PrimeVueDropdown', Dropdown);

app.mount('#app')
