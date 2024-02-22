import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { EnvService } from '../env.service';
import { CertificateAuthority } from '../model/certificate-authority';
import { FlatTreeControl } from '@angular/cdk/tree';
import { CollectionViewer, DataSource, SelectionChange } from '@angular/cdk/collections';
import { BehaviorSubject, Observable, merge } from 'rxjs';
import { map } from 'rxjs/operators';

interface Node {
  isLoading: boolean;
  level: number;
  name: string;
  hasChildren: boolean;
}

class CertificateAuthorityNode implements Node {
  isLoading = false;
  constructor(public certificateAuthority: CertificateAuthority, public level = 0) { }

  get name(): string {
    return this.certificateAuthority.name ?? "";
  }

  get hasChildren(): boolean {
    return this.certificateAuthority.links !== undefined
      && this.certificateAuthority.links["children"] !== undefined;
  }

  get childrenLink(): string {
    if (!this.hasChildren) {
      throw "Error";
    }

    return this.certificateAuthority.links!["children"];
  }
}

class AddCertificateAuthorityNode implements Node {
  isLoading = false;
  hasChildren = false;

  constructor(public parent: CertificateAuthority | null = null, public level = 0) { }

  get name() {
    return this.parent === null
      ? "Add root CA"
      : "Add intermediate CA";
  }
}

class CertificateAuthoritiesDataSource implements DataSource<Node> {
  dataChange = new BehaviorSubject<Node[]>([]);

  constructor(private _treeControl: FlatTreeControl<Node>, private _httpClient: HttpClient, private _envService: EnvService) { }

  get data(): Node[] {
    return this.dataChange.value;
  }

  set data(value: Node[]) {
    this._treeControl.dataNodes = value;
    this.dataChange.next(value);
  }

  connect(collectionViewer: CollectionViewer): Observable<Node[]> {
    this._treeControl.expansionModel.changed.subscribe(change => {
      if (
        (change as SelectionChange<Node>).added ||
        (change as SelectionChange<Node>).removed
      ) {
        this.handleTreeControl(change as SelectionChange<Node>);
      }
    });

    return merge(collectionViewer.viewChange, this.dataChange).pipe(map(() => this.data));
  }
  disconnect(): void { }

  /** Handle expand/collapse behaviors */
  handleTreeControl(change: SelectionChange<Node>) {
    if (change.added) {
      change.added.forEach(node => this.toggleNode(node, true));
    }
    if (change.removed) {
      change.removed
        .slice()
        .reverse()
        .forEach(node => this.toggleNode(node, false));
    }
  }

  /**
   * Toggle the node, remove from display list
   */
  toggleNode(node: Node, expand: boolean) {
    if (!expand) {
      this.removeChildren(node);
    }
    else if (node instanceof CertificateAuthorityNode) {
      this.expandNode(node);
    }

    // notify the change
    this.dataChange.next(this.data);
  }

  removeChildren(node: Node) {
    const index = this.data.indexOf(node);
    let count = 0;
    for (let i = index + 1; i < this.data.length && this.data[i].level > node.level; i++, count++) { }

    this.data.splice(index + 1, count);
  }

  expandNode(node: CertificateAuthorityNode) {
    if (node.hasChildren) {
      this.loadChildren(node);
    }
    else {
      const index = this.data.indexOf(node);
      this.data.splice(index + 1, 0, new AddCertificateAuthorityNode(node.certificateAuthority, node.level + 1));
    }
  }

  private loadChildren(node: CertificateAuthorityNode) {
    const childrenLink = node.childrenLink;

    node.isLoading = true;
    this._httpClient.get<CertificateAuthority[]>(`${this._envService.BACKEND_URI}${childrenLink}`).subscribe((children) => {

      const index = this.data.indexOf(node);
      const childNodes = children.map(child => new CertificateAuthorityNode(child, node.level + 1) as Node);
      childNodes.push(new AddCertificateAuthorityNode(node.certificateAuthority, node.level + 1));
      this.data.splice(index + 1, 0, ...childNodes);
      node.isLoading = false;
      this.dataChange.next(this.data);
    });
  }
}

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.less']
})
export class NavigationComponent implements OnInit {

  treeControl: FlatTreeControl<Node>
  dataSource: CertificateAuthoritiesDataSource
  certificateAuthorities!: CertificateAuthority[]

  constructor(private httpClient: HttpClient, private envService: EnvService) {
    this.treeControl = new FlatTreeControl<Node>(this.getLevel, this.isExpandable);
    this.dataSource = new CertificateAuthoritiesDataSource(this.treeControl, this.httpClient, this.envService);
  }

  ngOnInit(): void {
    this.httpClient.get<CertificateAuthority[]>(`${this.envService.BACKEND_URI}/cas?root=true`)
      .subscribe(cas => {
        const nodes = cas.map(ca => new CertificateAuthorityNode(ca) as Node);
        nodes.push(new AddCertificateAuthorityNode());
        this.dataSource.data = nodes;
      });
  }

  onClick(node: Node) {
    if (!(node instanceof AddCertificateAuthorityNode)) {
      return;
    }


  }

  getLevel = (node: Node) => node.level;

  isExpandable = (node: Node) => node.hasChildren;

  isAddNode = (_: number, node: Node) => node instanceof AddCertificateAuthorityNode;

  hasChildren = (_: number, node: Node) => node instanceof CertificateAuthorityNode;
}
