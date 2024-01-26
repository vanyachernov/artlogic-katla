import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HiveSectionListItem } from '../models/hive-section-list-item';
import { HiveSectionService } from '../services/hive-section.service';
import { HiveService } from '../services/hive.service';
import { HiveSection } from '../models/hive-section';

@Component({
  selector: 'app-hive-section-list',
  templateUrl: './hive-section-list.component.html',
  styleUrls: ['./hive-section-list.component.css']
})
export class HiveSectionListComponent implements OnInit {

  hiveId: number;
  hiveSections: Array<HiveSectionListItem>;
  selectedSection: HiveSection;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private hiveService: HiveService,
    private hiveSectionService: HiveSectionService
  ) { }

  ngOnInit() {
    this.route.params.subscribe(p => {
      this.hiveId = p['id'];
      this.hiveService.getHiveSections(this.hiveId).subscribe(s => this.hiveSections = s);
    })
  }

  editSection(section: HiveSection): void {
    this.selectedSection = section;
  }

  onDelete(sectionListId: number) {
    var section = this.hiveSections.find(h => h.id == sectionListId);
    this.hiveSectionService.setHiveSectionStatus(sectionListId, true).subscribe(() => {
      section.isDeleted = true;
    });
  }

  onUndelete(sectionListId: number) {
    var section = this.hiveSections.find(h => h.id == sectionListId);
    this.hiveSectionService.setHiveSectionStatus(sectionListId, false).subscribe(() => {
      section.isDeleted = false;
    });
  }
}
