<template>
    <PrimeVueButton @click="openAddDialog">Add new device</PrimeVueButton>

    <PrimeVueDialog header="Add new device" v-model:visible="displayAddDialog">
        <PrimeVueInputText v-model="inputName" type="text" class="p-inputtext" placeholder="Name" />
        <PrimeVueDropdown v-model="inputType"
                          :options="typeDropdownOptions"
                          optionLabel="name"
                          optionValue="code"
                          placeholder="Type" />
        <PrimeVueDropdown v-if="inputType === 'Doors' || inputType === 'Window'"
                          v-model="inputState"
                          :options="stateDropdownDoorsOptions"
                          optionLabel="name"
                          optionValue="code"
                          placeholder="State" />
        <PrimeVueDropdown :disabled="inputType === ''"
                          v-else v-model="inputState"
                          :options="stateDropdownElevatorOptions"
                          optionLabel="name"
                          optionValue="code"
                          placeholder="State" />

        <PrimeVueCheckbox id="automaticInput" v-model="inputIsAutomatic" :binary="true" />
        <label for="automaticInput">Automatic</label>

        <template #footer>
            <PrimeVueButton icon="pi pi-times" @click="closeAddDialog" class="p-button-text" />
            <PrimeVueButton label="Add" @click="addDevice" autofocus />
        </template>
    </PrimeVueDialog>

    <PrimeVueDialog header="Move elevator" v-model:visible="displayMoveDialog">
        <PrimeVueInputNumber :min="-1" :max="10" id="value" class="p-inputtext-sm" v-model="inputElevatorFloor" />
        <template #footer>
            <PrimeVueButton label="Move" @click="moveElevator" autofocus />
        </template>
    </PrimeVueDialog>

    <PrimeVueDialog header="Edit device" v-model:visible="displayEditDialog">
        <PrimeVueInputText v-model="inputName" type="text" class="p-inputtext" placeholder="Name" />
        <PrimeVueDropdown v-model="inputType"
                          :options="typeDropdownOptions"
                          optionLabel="name"
                          optionValue="code"
                          placeholder="Type" />
        <PrimeVueDropdown v-if="inputType === 'Doors' || inputType === 'Window'"
                          v-model="inputState"
                          :options="stateDropdownDoorsOptions"
                          optionLabel="name"
                          optionValue="code"
                          placeholder="State" />
        <PrimeVueDropdown :disabled="inputType === ''"
                          v-else v-model="inputState"
                          :options="stateDropdownElevatorOptions"
                          optionLabel="name"
                          optionValue="code"
                          placeholder="State" />

        <PrimeVueCheckbox id="automaticInput" v-model="inputIsAutomatic" :binary="true" />
        <label for="automaticInput">Automatic</label>

        <template #footer>
            <PrimeVueButton icon="pi pi-times" @click="closeEditDialog" class="p-button-text" />
            <PrimeVueButton label="Add" @click="editDevice" autofocus />
        </template>
    </PrimeVueDialog>

    <PrimeVueDataTable :value="data">
        <PrimeVueColumn field="name" header="Name"></PrimeVueColumn>
        <PrimeVueColumn header="Type">
            <template #body="slotProps">
                <i v-tooltip.right="'Doors'" v-if="slotProps.data.type === 'Doors'" class="pi pi-tablet"></i>
                <i v-tooltip.right="'Window'" v-if="slotProps.data.type === 'Window'" class="pi pi-stop"></i>
                <i v-tooltip.right="'Elevator'" v-if="slotProps.data.type === 'Elevator'" class="pi pi-arrow-circle-up"></i>
            </template>
        </PrimeVueColumn>
        <PrimeVueColumn field="state" header="State"></PrimeVueColumn>
        <PrimeVueColumn field="value" header="Value"></PrimeVueColumn>
        <PrimeVueColumn field="isAutomatic" header="Automatic"></PrimeVueColumn>
        <PrimeVueColumn header="Edit">
            <template #body="slotProps">
                <PrimeVueButton icon="pi pi-user-edit" iconPos="right" @click="openEditDialog(slotProps.data)"></PrimeVueButton>
            </template>
        </PrimeVueColumn>
        <PrimeVueColumn header="Delete">
            <template #body="slotProps">
                <PrimeVueButton icon="pi pi-trash" iconPos="right" @click="deleteDevice(slotProps.data)"></PrimeVueButton>
            </template>
        </PrimeVueColumn>
        <PrimeVueColumn header="Actions">
            <template #body="slotProps">
                <div v-if="slotProps.data.isAutomatic === false">
                    <div v-if="slotProps.data.type === 'Doors' || slotProps.data.type === 'Window'">
                        <PrimeVueButton icon="pi pi-lock" iconPos="right" v-if="slotProps.data.state === 'Open'" @click="closeDoorWindow(slotProps.data)"></PrimeVueButton>
                        <PrimeVueButton icon="pi pi-lock-open" iconPos="right" v-if="slotProps.data.state === 'Closed'" @click="openDoorWindow(slotProps.data)"></PrimeVueButton>
                    </div>
                    <div v-if="slotProps.data.type === 'Elevator'">
                        <PrimeVueButton icon="pi pi-arrows-v" iconPos="right" v-if="slotProps.data.state === 'Stationary'" @click="openMoveDialog(slotProps.data)"></PrimeVueButton>
                    </div>
                </div>
            </template>
        </PrimeVueColumn>

    </PrimeVueDataTable>
</template>

<script>
    export default {
        data() {
            return {
                loading: false,
                data: null,
                displayAddDialog: false,
                displayMoveDialog: false,
                displayEditDialog: false,
                inputName: '',
                inputType: '',
                inputState: '',
                inputValue: 0,
                inputIsAutomatic: false,
                typeDropdownOptions: [
                    { name: 'Doors', code: 'Doors' },
                    { name: 'Window', code: 'Window'},
                    { name: 'Elevator', code: 'Elevator'}
                ],
                stateDropdownDoorsOptions: [
                    { name: 'Closed', code: 'Closed' },
                    { name: 'Open', code: 'Open' }
                ],
                stateDropdownElevatorOptions: [
                    { name: 'Stationary', code: 'Stationary' },
                ],
                inputElevatorFloor: 0,
                editedDevice: null,
                movedDevice: null,
            };
        },
        created() {
            //to be replaced with signalR
            setInterval(this.fetchData, 5000)
        },
        watch: {
            '$route': 'fetchData'
        },
        methods: {
            fetchData() {
                this.data = null;
                this.loading = true;

                fetch('sdevices')
                    .then(r => r.json())
                    .then(json => {
                        this.data = json;
                        this.loading = false;
                        console.log(this.data)
                        return;
                    });
            },

            addDevice() {
                if ((this.inputType === 'Doors') || (this.inputType === 'Window')) {
                    if (this.inputState === 'Open') {
                        this.inputValue = 100
                    } else if (this.inputState === 'Closed') {
                        this.inputValue = 0
                    }
                }
                else if (this.inputType === 'Elevator') {
                    this.inputValue = 0
                } else {
                    console.log(this.inputType)
                }

                let newDevice = {
                    "name": this.inputName,
                    "type": this.inputType,
                    "state": this.inputState,
                    "value": this.inputValue,
                    "isAutomatic": this.inputIsAutomatic
                }
                fetch('sdevices', {
                    method: 'post',
                    body: JSON.stringify(newDevice),
                    headers: {
                        'content-type': 'application/json'
                    }
                })
                .then(() => {
                    this.fetchData()
                    this.closeAddDialog()
                })
                .catch(err => console.log(err))
            },

            openAddDialog() {
                this.inputName = "";
                this.inputType = "";
                this.inputState = "";
                this.inputValue = "";
                this.inputIsAutomatic = false;
                this.displayAddDialog = true;
            },

            closeAddDialog() {
                this.displayAddDialog = false;
            },

            closeDoorWindow(device) {
                fetch('sdevices/' + device.id + '/close')
                    .then(() => {
                        this.fetchData();
                    });
                this.displayMoveDialog = false;
            },
            
            openDoorWindow(device) {
                fetch('sdevices/' + device.id + '/open')
                    .then(() => {
                        this.fetchData();
                    });
                this.displayMoveDialog = false;
            },

            openMoveDialog(device) {
                this.inputElevatorFloor = device.value
                this.movedDevice = device
                this.displayMoveDialog = true;
            },

            editDevice() {
                if ((this.inputType === 'Doors') || (this.inputType === 'Window')) {
                    if (this.inputState === 'Open') {
                        this.inputValue = 100
                    } else if (this.inputState === 'Closed') {
                        this.inputValue = 0
                    }
                }
                else if (this.inputType === 'Elevator') {
                    this.inputValue = 0
                } else {
                    console.log(this.inputType)
                }

                let editedDevice = {
                    "name": this.inputName,
                    "type": this.inputType,
                    "state": this.inputState,
                    "value": this.inputValue,
                    "isAutomatic": this.inputIsAutomatic
                }
                fetch('sdevices/' + this.editedDevice.id, {
                    method: 'put',
                    body: JSON.stringify(editedDevice),
                    headers: {
                        'content-type': 'application/json'
                    }
                })
                    .then(() => {
                        this.fetchData()
                        this.closeEditDialog()
                    })
                    .catch(err => console.log(err))
            },

            deleteDevice(device) {
                fetch('sdevices/' + device.id, {
                    method: 'delete',
                })
                .then(() => {
                    this.fetchData()
                })
                .catch(err => console.log(err))
            },

            openEditDialog(device) {
                this.editedDevice = device
                this.inputName = device.name;
                this.inputType = device.type;
                this.inputState = device.state;
                this.inputValue = device.value;
                this.inputIsAutomatic = device.isAutomatic;
                this.displayEditDialog = true;
            },

            closeEditDialog() {
                this.displayEditDialog = false;
            },

            closeMoveDialog() {
                this.displayMoveDialog = false;
            },

            moveElevator() {
                fetch('sdevices/' + this.movedDevice.id + '/move_to/' + this.inputElevatorFloor)
                .then(() => {
                    this.fetchData();
                });
                this.displayMoveDialog = false;
            },
        },
    }
</script>

<style>
    PrimeVueButton {
        width: 20px;
        text-align:center;
    }
</style>