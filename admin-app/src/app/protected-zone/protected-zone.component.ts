import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-layout',
    templateUrl: './protected-zone.component.html',
    styleUrls: ['./protected-zone.component.scss']
})
export class ProtectedZoneComponent implements OnInit {
    collapedSideBar: boolean;

    constructor() {}

    ngOnInit() {}

    receiveCollapsed($event) {
        this.collapedSideBar = $event;
    }
}
